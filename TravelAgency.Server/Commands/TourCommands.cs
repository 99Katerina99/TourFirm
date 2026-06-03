using System.Text.Json;
using Microsoft.Data.Sqlite;
using TravelAgency.Core.Entities;
using TravelAgency.Core.Interfaces;
using TravelAgency.Server.Services;

namespace TravelAgency.Server.Commands;

public static class TourCommands
{
    // ============================================
    // --- ORM ВЕРСИЯ (через EF Core) ---
    // ============================================

    // GET_TOURS_ORM|token
    public static string GetToursOrm(string[] parts, IRepository<Tour> repo, SessionManager sessionManager)
    {
        if (parts.Length < 2 || !sessionManager.ValidateToken(parts[1], out _))
            return "ERROR|Требуется авторизация";

        var tours = repo.GetAllAsync().Result;
        return $"OK|{JsonSerializer.Serialize(tours)}";
    }

    // ADD_TOUR_ORM|token|Name|TourType|Destination|RouteId|StartDate|EndDate|Price|MaxSeats|Description
    public static string AddTourOrm(string[] parts, IRepository<Tour> repo, SessionManager sessionManager)
    {
        if (parts.Length < 10 || !sessionManager.ValidateToken(parts[1], out _))
            return "ERROR|Требуется авторизация или неверные параметры";

        try
        {
            var tour = new Tour
            {
                Name = parts[2],
                TourType = parts[3],              // "Круизный", "Курортный", "Бизнес"...
                Destination = parts[4],            // "Франция, Париж"
                RouteId = int.Parse(parts[5]),
                StartDate = DateTime.Parse(parts[6]),
                EndDate = DateTime.Parse(parts[7]),
                Price = decimal.Parse(parts[8]),
                MaxSeats = int.Parse(parts[9]),
                Description = parts.Length > 10 ? parts[10] : string.Empty
            };

            repo.AddAsync(tour).Wait();
            return $"OK|Тур добавлен (ORM). ID: {tour.Id}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ORM Error: {ex.Message}");
            return $"ERROR|Ошибка добавления тура: {ex.Message}";
        }
    }

    // ============================================
    // --- SQL ВЕРСИЯ (через ADO.NET) ---
    // ============================================

    // GET_TOURS_SQL|token
    public static string GetToursSql(string[] parts, IRepository<Tour> repo, SessionManager sessionManager)
    {
        if (parts.Length < 2 || !sessionManager.ValidateToken(parts[1], out _))
            return "ERROR|Требуется авторизация";

        var tours = repo.GetAllAsync().Result;
        return $"OK|{JsonSerializer.Serialize(tours)}";
    }

    // ADD_TOUR_SQL|token|Name|TourType|Destination|RouteId|StartDate|EndDate|Price|MaxSeats|Description
    public static string AddTourSql(string[] parts, string connectionString, SessionManager sessionManager)
    {
        // 1. Валидация токена и параметров
        if (parts.Length < 10 || !sessionManager.ValidateToken(parts[1], out _))
            return "ERROR|Требуется авторизация или неверные параметры";

        // 2. Парсинг данных под новую структуру БД
        var tour = new Tour
        {
            Name = parts[2],
            TourType = parts[3],
            Destination = parts[4],
            RouteId = int.Parse(parts[5]),
            StartDate = DateTime.Parse(parts[6]),
            EndDate = DateTime.Parse(parts[7]),
            Price = decimal.Parse(parts[8]),
            MaxSeats = int.Parse(parts[9]),
            Description = parts.Length > 10 ? parts[10] : string.Empty
        };

        // 3. ПРЯМОЙ SQL-ЗАПРОС (демонстрация Raw SQL)
        try
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();

            // 🔥 SQL-запрос под новую структуру таблицы Tours
            cmd.CommandText = @"
                INSERT INTO Tours 
                (RouteId, Name, TourType, Destination, StartDate, EndDate, Price, MaxSeats, Description) 
                VALUES 
                (@RouteId, @Name, @TourType, @Destination, @StartDate, @EndDate, @Price, @MaxSeats, @Description);
                SELECT last_insert_rowid();";

            // Параметры — защита от SQL-инъекций
            cmd.Parameters.AddWithValue("@RouteId", tour.RouteId);
            cmd.Parameters.AddWithValue("@Name", tour.Name);
            cmd.Parameters.AddWithValue("@TourType", tour.TourType);
            cmd.Parameters.AddWithValue("@Destination", (object?)tour.Destination ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StartDate", tour.StartDate);
            cmd.Parameters.AddWithValue("@EndDate", tour.EndDate);
            cmd.Parameters.AddWithValue("@Price", tour.Price);
            cmd.Parameters.AddWithValue("@MaxSeats", tour.MaxSeats);
            cmd.Parameters.AddWithValue("@Description", (object?)tour.Description ?? DBNull.Value);

            // Выполнение и получение нового ID
            var newId = Convert.ToInt32(cmd.ExecuteScalar());
            tour.Id = newId;

            return $"OK|Тур добавлен (Raw SQL). ID: {newId}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ SQL Error: {ex.Message}");
            return $"ERROR|Ошибка БД: {ex.Message}";
        }
    }

    // ============================================
    // --- НОВЫЕ КОМАНДЫ ДЛЯ ТЗ ---
    // ============================================

    // GET_ROUTES|token — получить список маршрутов
    public static string GetRoutes(string[] parts, IRepository<Route> repo, SessionManager sessionManager)
    {
        if (parts.Length < 2 || !sessionManager.ValidateToken(parts[1], out _))
            return "ERROR|Требуется авторизация";

        var routes = repo.GetAllAsync().Result;
        return $"OK|{JsonSerializer.Serialize(routes)}";
    }

    // GET_CLIENTS|token — получить список клиентов
    public static string GetClients(string[] parts, IRepository<Client> repo, SessionManager sessionManager)
    {
        if (parts.Length < 2 || !sessionManager.ValidateToken(parts[1], out _))
            return "ERROR|Требуется авторизация";

        var clients = repo.GetAllAsync().Result;
        return $"OK|{JsonSerializer.Serialize(clients)}";
    }

    // REGISTER_CLIENT|token|clientId|tourId — регистрация клиента на тур
    public static string RegisterClient(string[] parts,
                                        IRepository<Client> clientRepo,
                                        IRepository<Tour> tourRepo,
                                        SessionManager sessionManager)
    {
        if (parts.Length < 4 || !sessionManager.ValidateToken(parts[1], out _))
            return "ERROR|Требуется авторизация или неверные параметры";

        try
        {
            var clientId = int.Parse(parts[2]);
            var tourId = int.Parse(parts[3]);

            var client = clientRepo.GetByIdAsync(clientId).Result;
            var tour = tourRepo.GetByIdAsync(tourId).Result;

            if (client == null || tour == null)
                return "ERROR|Клиент или тур не найдены";

            // Автоматическая генерация номера договора
            var contractNumber = $"CN-{DateTime.Now:yyyyMMdd}-{clientId:D4}";

            return $"OK|Клиент {client.LastName} {client.FirstName} зарегистрирован на тур '{tour.Name}'. " +
                   $"Договор: {contractNumber}, Стоимость: {tour.Price:C}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Registration Error: {ex.Message}");
            return $"ERROR|Ошибка регистрации: {ex.Message}";
        }
    }

    // CREATE_TOUR_FULL|token|Name|TourType|Destination|RouteId|StartDate|EndDate|Price|MaxSeats|Description
    //                  |ClientLastName|ClientFirstName|ClientPhone|SupplierId|PaymentMethod|Amount
    public static string CreateTourFull(string[] parts, string connectionString, SessionManager sessionManager)
    {
        // 1. Минимальная валидация (ожидается 17 параметров)
        if (parts.Length < 17 || !sessionManager.ValidateToken(parts[1], out _))
            return "ERROR|Требуется авторизация или неверные параметры";

        try
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            using var tx = conn.BeginTransaction(); //  Транзакция: всё или ничего

            try
            {
                // ---- 1. Создаём Tour ----
                using var cmdTour = conn.CreateCommand();
                cmdTour.Transaction = tx;
                cmdTour.CommandText = @"
                INSERT INTO Tours (RouteId, Name, TourType, Destination, StartDate, EndDate, Price, MaxSeats, Description)
                VALUES (@RouteId, @Name, @TourType, @Destination, @StartDate, @EndDate, @Price, @MaxSeats, @Description);
                SELECT last_insert_rowid();";
                cmdTour.Parameters.AddWithValue("@RouteId", int.Parse(parts[5]));
                cmdTour.Parameters.AddWithValue("@Name", parts[2]);
                cmdTour.Parameters.AddWithValue("@TourType", parts[3]);
                cmdTour.Parameters.AddWithValue("@Destination", parts[4]);
                cmdTour.Parameters.AddWithValue("@StartDate", DateTime.Parse(parts[6]));
                cmdTour.Parameters.AddWithValue("@EndDate", DateTime.Parse(parts[7]));
                cmdTour.Parameters.AddWithValue("@Price", decimal.Parse(parts[8]));
                cmdTour.Parameters.AddWithValue("@MaxSeats", int.Parse(parts[9]));
                cmdTour.Parameters.AddWithValue("@Description", parts[10]);
                var tourId = Convert.ToInt32(cmdTour.ExecuteScalar());

                // ---- 2. Ищем или создаём Client ----
                var clientLastName = parts[11];
                var clientFirstName = parts[12];
                var clientPhone = parts[13];

                using var cmdFindClient = conn.CreateCommand();
                cmdFindClient.Transaction = tx;
                cmdFindClient.CommandText = "SELECT Id FROM Clients WHERE LastName = @LN AND FirstName = @FN AND Phone = @Ph";
                cmdFindClient.Parameters.AddWithValue("@LN", clientLastName);
                cmdFindClient.Parameters.AddWithValue("@FN", clientFirstName);
                cmdFindClient.Parameters.AddWithValue("@Ph", clientPhone);
                var existingClientId = cmdFindClient.ExecuteScalar();

                int clientId;
                if (existingClientId != null)
                {
                    clientId = Convert.ToInt32(existingClientId);
                }
                else
                {
                    using var cmdClient = conn.CreateCommand();
                    cmdClient.Transaction = tx;
                    cmdClient.CommandText = @"
                    INSERT INTO Clients (LastName, FirstName, MiddleName, PassportSeries, PassportNumber, 
                                         PassportIssuedBy, PassportIssuedDate, Phone, Email, RegistrationDate)
                    VALUES (@LN, @FN, @MN, @PS, @PN, @PBy, @PDate, @Ph, @Email, @RegDate);
                    SELECT last_insert_rowid();";
                    cmdClient.Parameters.AddWithValue("@LN", clientLastName);
                    cmdClient.Parameters.AddWithValue("@FN", clientFirstName);
                    cmdClient.Parameters.AddWithValue("@MN", parts.Length > 17 && !string.IsNullOrWhiteSpace(parts[17]) ? parts[17] : "");
                    cmdClient.Parameters.AddWithValue("@PS", "0000");
                    cmdClient.Parameters.AddWithValue("@PN", "000000");
                    cmdClient.Parameters.AddWithValue("@PBy", "Неизвестно");
                    cmdClient.Parameters.AddWithValue("@PDate", DateTime.UtcNow);
                    cmdClient.Parameters.AddWithValue("@Ph", clientPhone);
                    
                    var randomEmail = $"user{new Random().Next(1, 99999999)}@placeholder.com";
                    cmdClient.Parameters.AddWithValue("@Email", randomEmail);
                    cmdClient.Parameters.AddWithValue("@RegDate", DateTime.UtcNow);
                    clientId = Convert.ToInt32(cmdClient.ExecuteScalar());
                }

                // ---- 3. Создаём Request ----
                using var cmdReq = conn.CreateCommand();
                cmdReq.Transaction = tx;
                cmdReq.CommandText = @"
                INSERT INTO Requests (ClientId, RequestDate, Status, Wishes)
                VALUES (@ClientId, @Date, @Status, @Wishes);
                SELECT last_insert_rowid();";
                cmdReq.Parameters.AddWithValue("@ClientId", clientId);
                cmdReq.Parameters.AddWithValue("@Date", DateTime.UtcNow);
                cmdReq.Parameters.AddWithValue("@Status", "Подтверждена");
                cmdReq.Parameters.AddWithValue("@Wishes", parts[10]);
                var requestId = Convert.ToInt32(cmdReq.ExecuteScalar());

                // ---- 4. Создаём ClientContract ----
                var contractNumber = $"CN-{DateTime.Now:yyyyMMdd}-{clientId:D4}";
                using var cmdCC = conn.CreateCommand();
                cmdCC.Transaction = tx;
                cmdCC.CommandText = @"
                INSERT INTO ClientContracts (RequestId, TourId, ContractDate, ContractNumber, Status, TotalCost)
                VALUES (@ReqId, @TourId, @Date, @Num, @Status, @Cost);
                SELECT last_insert_rowid();";
                cmdCC.Parameters.AddWithValue("@ReqId", requestId);
                cmdCC.Parameters.AddWithValue("@TourId", tourId);
                cmdCC.Parameters.AddWithValue("@Date", DateTime.UtcNow);
                cmdCC.Parameters.AddWithValue("@Num", contractNumber);
                cmdCC.Parameters.AddWithValue("@Status", "Подписан");
                cmdCC.Parameters.AddWithValue("@Cost", decimal.Parse(parts[8]));
                var clientContractId = Convert.ToInt32(cmdCC.ExecuteScalar());

                // ---- 5. Создаём SupplierContract ----
                var supplierId = int.Parse(parts[14]);
                using var cmdSC = conn.CreateCommand();
                cmdSC.Transaction = tx;
                cmdSC.CommandText = @"
                INSERT INTO SupplierContracts (SupplierId, TourId, ContractDate, Service, ConfirmationStatus, Cost)
                VALUES (@SupId, @TourId, @Date, @Service, @Status, @Cost);
                SELECT last_insert_rowid();";
                cmdSC.Parameters.AddWithValue("@SupId", supplierId);
                cmdSC.Parameters.AddWithValue("@TourId", tourId);
                cmdSC.Parameters.AddWithValue("@Date", DateTime.UtcNow);
                cmdSC.Parameters.AddWithValue("@Service", "Полный пакет услуг");
                cmdSC.Parameters.AddWithValue("@Status", "Подтверждён");
                cmdSC.Parameters.AddWithValue("@Cost", decimal.Parse(parts[8]));
                var supplierContractId = Convert.ToInt32(cmdSC.ExecuteScalar());

                // ---- 6. Создаём Booking ----
                var bookingNum = $"BK-{DateTime.Now:yyyyMMddHHmmss}";
                using var cmdBook = conn.CreateCommand();
                cmdBook.Transaction = tx;
                cmdBook.CommandText = @"
                INSERT INTO Bookings (ClientContractId, SupplierContractId, BookingNum, BookingDate, Status)
                VALUES (@CCId, @SCId, @Num, @Date, @Status);
                SELECT last_insert_rowid();";
                cmdBook.Parameters.AddWithValue("@CCId", clientContractId);
                cmdBook.Parameters.AddWithValue("@SCId", supplierContractId);
                cmdBook.Parameters.AddWithValue("@Num", bookingNum);
                cmdBook.Parameters.AddWithValue("@Date", DateTime.UtcNow);
                cmdBook.Parameters.AddWithValue("@Status", "Подтверждено");
                var bookingId = Convert.ToInt32(cmdBook.ExecuteScalar());

                // ---- 7. Создаём Payment ----
                using var cmdPay = conn.CreateCommand();
                cmdPay.Transaction = tx;
                cmdPay.CommandText = @"
                INSERT INTO Payments (ClientContractId, Amount, PaymentDate, PaymentMethod, PaymentStatus)
                VALUES (@CCId, @Amount, @Date, @Method, @Status);
                SELECT last_insert_rowid();";
                cmdPay.Parameters.AddWithValue("@CCId", clientContractId);
                cmdPay.Parameters.AddWithValue("@Amount", decimal.Parse(parts[16]));
                cmdPay.Parameters.AddWithValue("@Date", DateTime.UtcNow);
                cmdPay.Parameters.AddWithValue("@Method", parts[15]);
                cmdPay.Parameters.AddWithValue("@Status", "Оплачен");
                var paymentId = Convert.ToInt32(cmdPay.ExecuteScalar());

                tx.Commit();

                return $"OK|Тур '{parts[2]}' создан. Клиент: {clientLastName} {clientFirstName}. " +
                       $"Договор №{contractNumber}. Бронирование №{bookingNum}. ID платежа: {paymentId}";
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ SQL Error: {ex.Message}");
            return $"ERROR|Ошибка БД: {ex.Message}";
        }
    }

    // GET_SUPPLIERS|token
    public static string GetSuppliers(string[] parts, string connectionString, SessionManager sessionManager)
    {
        if (parts.Length < 2 || !sessionManager.ValidateToken(parts[1], out _))
            return "ERROR|Требуется авторизация";

        var suppliers = new List<object>();
        using var conn = new SqliteConnection(connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, SupplierType FROM Suppliers";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            suppliers.Add(new
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Type = reader.IsDBNull(2) ? "" : reader.GetString(2)
            });
        }
        return $"OK|{JsonSerializer.Serialize(suppliers)}";
    }

    // GET_CLIENTS|token
    public static string GetClients(string[] parts, string connectionString, SessionManager sessionManager)
    {
        if (parts.Length < 2 || !sessionManager.ValidateToken(parts[1], out _))
            return "ERROR|Требуется авторизация";

        var clients = new List<object>();
        using var conn = new SqliteConnection(connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, LastName, FirstName, Phone FROM Clients";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            clients.Add(new
            {
                Id = reader.GetInt32(0),
                Name = $"{reader.GetString(1)} {reader.GetString(2)}",
                Phone = reader.IsDBNull(3) ? "" : reader.GetString(3)
            });
        }
        return $"OK|{JsonSerializer.Serialize(clients)}";
    }

    // DELETE_TOUR|token|tourId
    public static string DeleteTour(string[] parts, string connectionString, SessionManager sessionManager)
    {
        if (parts.Length < 3 || !sessionManager.ValidateToken(parts[1], out _))
            return "ERROR|Требуется авторизация или неверные параметры";

        try
        {
            var tourId = int.Parse(parts[2]);

            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            using var tx = conn.BeginTransaction();
            try
            {
                // Удаляем связанные записи в правильном порядке
                using var cmd1 = conn.CreateCommand();
                cmd1.Transaction = tx;
                cmd1.CommandText = @"
                DELETE FROM Payments WHERE ClientContractId IN 
                    (SELECT Id FROM ClientContracts WHERE TourId = @Tid);
                DELETE FROM Bookings WHERE ClientContractId IN 
                    (SELECT Id FROM ClientContracts WHERE TourId = @Tid);
                DELETE FROM SupplierContracts WHERE TourId = @Tid;
                DELETE FROM ClientContracts WHERE TourId = @Tid;
                DELETE FROM Requests WHERE Id IN 
                    (SELECT RequestId FROM ClientContracts WHERE TourId = @Tid);
                DELETE FROM Tours WHERE Id = @Tid;";
                cmd1.Parameters.AddWithValue("@Tid", tourId);
                var affected = cmd1.ExecuteNonQuery();
                tx.Commit();

                return $"OK|Тур ID={tourId} удалён. Затронуто записей: {affected}";
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ SQL Error: {ex.Message}");
            return $"ERROR|Ошибка БД: {ex.Message}";
        }
    }

    // GET_SUPPLIER_CONTRACTS|token
    public static string GetSupplierContracts(string[] parts, string connectionString, SessionManager sessionManager)
    {
        if (parts.Length < 2 || !sessionManager.ValidateToken(parts[1], out _))
            return "ERROR|Требуется авторизация";

        var contracts = new List<object>();
        using var conn = new SqliteConnection(connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        SELECT 
            sc.Id,
            sc.ContractDate,
            sc.Service,
            sc.ConfirmationStatus,
            sc.Cost,
            t.Name as TourName,
            s.Name as SupplierName
        FROM SupplierContracts sc
        JOIN Tours t ON sc.TourId = t.Id
        JOIN Suppliers s ON sc.SupplierId = s.Id
        ORDER BY sc.ContractDate DESC";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            contracts.Add(new
            {
                Id = reader.GetInt32(0),
                ContractNumber = $"SC-{reader.GetDateTime(1):yyyyMMdd}-{reader.GetInt32(0):D4}",
                TourName = reader.GetString(5),
                SupplierName = reader.GetString(6),
                Service = reader.IsDBNull(2) ? "" : reader.GetString(2),
                Cost = reader.GetDecimal(4),
                ConfirmationStatus = reader.IsDBNull(3) ? "" : reader.GetString(3),
                ContractDate = reader.GetDateTime(1)
            });
        }
        return $"OK|{JsonSerializer.Serialize(contracts)}";
    }

    // GET_TOURS_EXTENDED|token
    public static string GetToursExtended(string[] parts, string connectionString, SessionManager sessionManager)
    {
        if (parts.Length < 2 || !sessionManager.ValidateToken(parts[1], out _))
            return "ERROR|Требуется авторизация";

        var tours = new List<Core.Entities.TourExtendedDto>();

        using var conn = new SqliteConnection(connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        SELECT 
            t.Id as TourId,
            t.Name as TourName,
            t.TourType,
            t.Destination,
            t.StartDate,
            t.EndDate,
            t.Price,
            t.MaxSeats,
            
            c.LastName || ' ' || c.FirstName || ' ' || COALESCE(c.MiddleName, '') as ClientName,
            c.Phone as ClientPhone,
            
            cc.ContractNumber as ClientContractNumber,
            cc.Status as ClientContractStatus,
            
            s.Name as SupplierName,
            'SC-' || strftime('%Y%m%d', sc.ContractDate) || '-' || printf('%04d', sc.Id) as SupplierContractNumber,
            
            b.BookingNum as BookingNumber,
            b.Status as BookingStatus,
            
            p.PaymentMethod,
            p.PaymentStatus,
            p.Amount as PaymentAmount
            
        FROM Tours t
        LEFT JOIN ClientContracts cc ON t.Id = cc.TourId
        LEFT JOIN Requests r ON cc.RequestId = r.Id
        LEFT JOIN Clients c ON r.ClientId = c.Id
        LEFT JOIN SupplierContracts sc ON t.Id = sc.TourId
        LEFT JOIN Suppliers s ON sc.SupplierId = s.Id
        LEFT JOIN Bookings b ON cc.Id = b.ClientContractId AND sc.Id = b.SupplierContractId
        LEFT JOIN Payments p ON cc.Id = p.ClientContractId
        ORDER BY t.StartDate DESC";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            tours.Add(new Core.Entities.TourExtendedDto
            {
                TourId = reader.GetInt32(0),
                TourName = reader.GetString(1),
                TourType = reader.GetString(2),
                Destination = reader.GetString(3),
                StartDate = reader.GetDateTime(4),
                EndDate = reader.GetDateTime(5),
                Price = reader.GetDecimal(6),
                MaxSeats = reader.GetInt32(7),

                ClientName = reader.IsDBNull(8) ? null : reader.GetString(8),
                ClientPhone = reader.IsDBNull(9) ? null : reader.GetString(9),

                ClientContractNumber = reader.IsDBNull(10) ? null : reader.GetString(10),
                ClientContractStatus = reader.IsDBNull(11) ? null : reader.GetString(11),

                SupplierName = reader.IsDBNull(12) ? null : reader.GetString(12),
                SupplierContractNumber = reader.IsDBNull(13) ? null : reader.GetString(13),

                BookingNumber = reader.IsDBNull(14) ? null : reader.GetString(14),
                BookingStatus = reader.IsDBNull(15) ? null : reader.GetString(15),

                PaymentMethod = reader.IsDBNull(16) ? null : reader.GetString(16),
                PaymentStatus = reader.IsDBNull(17) ? null : reader.GetString(17),
                PaymentAmount = reader.IsDBNull(18) ? null : reader.GetDecimal(18)
            });
        }

        return $"OK|{JsonSerializer.Serialize(tours)}";
    }
}