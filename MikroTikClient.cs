using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using tik4net;
using tik4net.Objects;
using tik4net.Objects.Ppp;

namespace MikrNSN
{
    public static class MikroTikClient
    {
        private static ITikConnection _connection = null!;
        private static string _host = "";

        public static async Task<bool> ConnectAsync(string host, string user, string pass, int port, bool tls)
        {
            if (_connection != null && _connection.IsOpened) return true;
            try
            {
                _host = host ?? "";
                var targetHost = _host.Trim();
                if (string.IsNullOrEmpty(targetHost)) throw new ArgumentException("حقل المضيف (host) فارغ أو غير صالح");
                try
                {
                    var ping = new Ping();
                    await Task.Run(() => ping.Send(targetHost, 1200));
                }
                catch { }

                var probed = await ProbePortAsync(targetHost, port, 1500);
                if (!probed) throw new Exception("لا يمكن الوصول إلى المنفذ المحدد");
                var defaultPort = tls ? 8729 : 8728;
                if (port == defaultPort)
                {
                    _connection = ConnectionFactory.OpenConnection(tls ? TikConnectionType.ApiSsl : TikConnectionType.Api, targetHost, user, pass);
                    return _connection.IsOpened;
                }
                _connection = ConnectionFactory.CreateConnection(tls ? TikConnectionType.ApiSsl : TikConnectionType.Api);
                _connection.Open(targetHost, port, user, pass);
                return _connection.IsOpened;
            }
            catch (SocketException ex)
            {
                throw new Exception(MapSocketError(ex.SocketErrorCode));
            }
            catch (Exception ex)
            {
                var msg = ex.Message?.ToLowerInvariant() ?? "";
                if (msg.Contains("login") || msg.Contains("auth")) throw new Exception("فشل تسجيل الدخول: تحقق من اسم المستخدم وكلمة السر");
                if (tls) throw new Exception("فشل اتصال TLS: تأكد من تفعيل api-ssl وربط شهادة وبورت 8729");
                throw new Exception("فشل الاتصال: تأكد من تفعيل خدمة API والبورت والجدار الناري");
            }
        }

        private static async Task<bool> ProbePortAsync(string host, int port, int timeoutMs)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(host)) return false;
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(host, port);
                var timeoutTask = Task.Delay(timeoutMs);
                var finished = await Task.WhenAny(connectTask, timeoutTask);
                if (finished == timeoutTask) return false;
                return client.Connected;
            }
            catch { return false; }
        }

        private static string MapSocketError(SocketError code)
        {
            return code switch
            {
                SocketError.ConnectionRefused => "المنفذ مغلق أو الخدمة غير مفعلة",
                SocketError.TimedOut => "انتهى وقت الاتصال: تحقق من الشبكة",
                SocketError.NetworkUnreachable => "الشبكة غير متاحة",
                SocketError.HostUnreachable => "المضيف غير متاح",
                _ => "خطأ في الشبكة أثناء الاتصال"
            };
        }

        public static Task<string> GetIdentityAsync()
        {
            return Task.Run(() =>
            {
                if (_connection == null || !_connection.IsOpened) throw new InvalidOperationException("Not connected");
                var cmd = _connection.CreateCommand("/system/identity/print");
                var identity = cmd.ExecuteScalar();
                return identity?.ToString() ?? "RouterOS";
            });
        }

        public static string CurrentHost => _host;

        public class UmRouter
        {
            public string Name { get; set; } = "";
            public string IpAddress { get; set; } = "";
            public string SharedSecret { get; set; } = "";
            public string CoaPort { get; set; } = "";
            public string TimeZone { get; set; } = "";
        }

        public static List<UmRouter> GetUsermanRouters()
        {
            try
            {
                if (_connection == null || !_connection.IsOpened) return new List<UmRouter>();
                var cmd = _connection.CreateCommand("/userman/router/print");
                var rows = cmd.ExecuteList();
                if (rows.Count() == 0)
                {
                    cmd = _connection.CreateCommand("/tool/user-manager/router/print");
                    rows = cmd.ExecuteList();
                }
                var list = new List<UmRouter>();
                foreach (var r in rows)
                {
                    list.Add(new UmRouter
                    {
                        Name = r.GetResponseField("name") ?? r.GetResponseField("router-name"),
                        IpAddress = r.GetResponseField("address") ?? r.GetResponseField("ip-address"),
                        SharedSecret = r.GetResponseField("shared-secret"),
                        CoaPort = r.GetResponseField("coa-port"),
                        TimeZone = r.GetResponseField("time-zone") ?? r.GetResponseField("timezone")
                    });
                }
                return list;
            }
            catch { return new List<UmRouter>(); }
        }

        public static bool IsUsermanRouterConfiguredForCurrent()
        {
            try
            {
                var routers = GetUsermanRouters();
                var host = CurrentHost;
                var candidate = routers.FirstOrDefault(r => string.Equals(r.IpAddress, host, StringComparison.OrdinalIgnoreCase) || string.Equals(r.Name, host, StringComparison.OrdinalIgnoreCase));
                candidate ??= routers.FirstOrDefault();
                if (candidate == null) return false;
                var hasIp = !string.IsNullOrWhiteSpace(candidate.IpAddress);
                var hasSecret = !string.IsNullOrWhiteSpace(candidate.SharedSecret);
                var hasCoa = !string.IsNullOrWhiteSpace(candidate.CoaPort);
                var hasTz = !string.IsNullOrWhiteSpace(candidate.TimeZone);
                return hasIp && hasSecret && hasCoa && hasTz;
            }
            catch { return false; }
        }

        public class UserManagerProfile
        {
            public string Name { get; set; } = "";
            public string RateLimit { get; set; } = "";
            public string Validity { get; set; } = "";
            public string Quota { get; set; } = "";
            public string Commission { get; set; } = "";
            public string UsersAllowed { get; set; } = "";
            public string Price { get; set; } = "";
            public string Comment { get; set; } = "";
            public string Owner { get; set; } = "";
            public string StartsAt { get; set; } = "";
            public string OverrideRateLimit { get; set; } = "";
            public string Note { get; set; } = "";
        }

        public static List<UserManagerProfile> GetUserManagerProfiles()
        {
            try
            {
                if (_connection == null || !_connection.IsOpened) return new List<UserManagerProfile>();
                var cmd = _connection.CreateCommand("/userman/profile/print");
                var rows = cmd.ExecuteList();
                if (rows.Count() == 0)
                {
                    cmd = _connection.CreateCommand("/tool/user-manager/profile/print");
                    rows = cmd.ExecuteList();
                }
                var list = new List<UserManagerProfile>();
                foreach (var r in rows)
                {
                    list.Add(new UserManagerProfile
                    {
                        Name = r.GetResponseField("name"),
                        RateLimit = r.GetResponseField("rate-limit"),
                        Validity = r.GetResponseField("validity"),
                        Quota = r.GetResponseField("download-limit"),
                        Commission = r.GetResponseField("commission"),
                        UsersAllowed = r.GetResponseField("customer-count"),
                        Price = r.GetResponseField("price"),
                        Comment = r.GetResponseField("comment"),
                        Owner = r.GetResponseField("owner"),
                        StartsAt = r.GetResponseField("starts-at"),
                        OverrideRateLimit = r.GetResponseField("override-rate-limit"),
                        Note = r.GetResponseField("note"),
                    });
                }
                return list;
            }
            catch { return new List<UserManagerProfile>(); }
        }

        public class ProfileLimit
        {
            public string Profile { get; set; } = "";
            public string DownloadLimit { get; set; } = "";
            public string UploadLimit { get; set; } = "";
            public string TransferLimit { get; set; } = "";
            public string TimeLimit { get; set; } = "";
        }

        public static List<ProfileLimit> GetProfileLimits()
        {
            try
            {
                if (_connection == null || !_connection.IsOpened) return new List<ProfileLimit>();
                var cmd = _connection.CreateCommand("/userman/limitation/print");
                var rows = cmd.ExecuteList();
                if (rows.Count() == 0)
                {
                    cmd = _connection.CreateCommand("/tool/user-manager/profile/limit/print");
                    rows = cmd.ExecuteList();
                }
                var list = new List<ProfileLimit>();
                foreach (var r in rows)
                {
                    list.Add(new ProfileLimit
                    {
                        Profile = r.GetResponseField("profile"),
                        DownloadLimit = r.GetResponseField("download-limit"),
                        UploadLimit = r.GetResponseField("upload-limit"),
                        TransferLimit = r.GetResponseField("transfer-limit"),
                        TimeLimit = r.GetResponseField("time-limit"),
                    });
                }
                return list;
            }
            catch { return new List<ProfileLimit>(); }
        }

        public class UserProfileAssignment
        {
            public string User { get; set; } = "";
            public string Profile { get; set; } = "";
            public string Created { get; set; } = "";
            public string Owner { get; set; } = "";
        }

        public static List<UserProfileAssignment> GetUserProfileAssignments()
        {
            try
            {
                if (_connection == null || !_connection.IsOpened) return new List<UserProfileAssignment>();
                var cmd = _connection.CreateCommand("/userman/user/profile/print");
                var rows = cmd.ExecuteList();
                if (rows.Count() == 0)
                {
                    cmd = _connection.CreateCommand("/tool/user-manager/user-profile/print");
                    rows = cmd.ExecuteList();
                }
                var list = new List<UserProfileAssignment>();
                foreach (var r in rows)
                {
                    list.Add(new UserProfileAssignment
                    {
                        User = r.GetResponseField("user"),
                        Profile = r.GetResponseField("profile"),
                        Created = r.GetResponseField("created"),
                        Owner = r.GetResponseField("owner"),
                    });
                }
                return list;
            }
            catch { return new List<UserProfileAssignment>(); }
        }

        public static void AddUserManagerProfile(string name, string rateLimit, string transferLimit, string validity, string price, string note)
        {
            if (_connection == null || !_connection.IsOpened) throw new InvalidOperationException("Not connected");
            try
            {
                var add = _connection.CreateCommandAndParameters("/userman/profile/add",
                    "name", name,
                    "rate-limit", rateLimit,
                    "validity", validity,
                    "price", price,
                    "note", note);
                add.ExecuteNonQuery();
                if (!string.IsNullOrWhiteSpace(transferLimit))
                {
                    var addLimit = _connection.CreateCommandAndParameters("/userman/limitation/add",
                        "profile", name,
                        "transfer-limit", transferLimit);
                    addLimit.ExecuteNonQuery();
                }
            }
            catch
            {
                var add2 = _connection.CreateCommandAndParameters("/tool/user-manager/profile/add",
                    "name", name,
                    "rate-limit", rateLimit,
                    "validity", validity,
                    "price", price,
                    "note", note);
                add2.ExecuteNonQuery();
                if (!string.IsNullOrWhiteSpace(transferLimit))
                {
                    var addLimit2 = _connection.CreateCommandAndParameters("/tool/user-manager/profile/limit/add",
                        "profile", name,
                        "transfer-limit", transferLimit);
                    addLimit2.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateUserManagerProfile(string name, string rateLimit, string transferLimit, string validity, string price, string note)
        {
            if (_connection == null || !_connection.IsOpened) throw new InvalidOperationException("Not connected");
            try
            {
                var find = _connection.CreateCommandAndParameters("/userman/profile/print", "name", name);
                var id = find.ExecuteList().FirstOrDefault()?.GetId();
                if (!string.IsNullOrEmpty(id))
                {
                    var set = _connection.CreateCommandAndParameters("/userman/profile/set",
                        TikSpecialProperties.Id, id,
                        "rate-limit", rateLimit,
                        "validity", validity,
                        "price", price,
                        "note", note);
                    set.ExecuteNonQuery();
                }
                if (!string.IsNullOrWhiteSpace(transferLimit))
                {
                    var limFind = _connection.CreateCommandAndParameters("/userman/limitation/print", "profile", name);
                    var limId = limFind.ExecuteList().FirstOrDefault()?.GetId();
                    if (!string.IsNullOrEmpty(limId))
                    {
                        var limSet = _connection.CreateCommandAndParameters("/userman/limitation/set",
                            TikSpecialProperties.Id, limId,
                            "transfer-limit", transferLimit);
                        limSet.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                var find2 = _connection.CreateCommandAndParameters("/tool/user-manager/profile/print", "name", name);
                var id2 = find2.ExecuteList().FirstOrDefault()?.GetId();
                if (!string.IsNullOrEmpty(id2))
                {
                    var set2 = _connection.CreateCommandAndParameters("/tool/user-manager/profile/set",
                        TikSpecialProperties.Id, id2,
                        "rate-limit", rateLimit,
                        "validity", validity,
                        "price", price,
                        "note", note);
                    set2.ExecuteNonQuery();
                }
                if (!string.IsNullOrWhiteSpace(transferLimit))
                {
                    var limFind2 = _connection.CreateCommandAndParameters("/tool/user-manager/profile/limit/print", "profile", name);
                    var limId2 = limFind2.ExecuteList().FirstOrDefault()?.GetId();
                    if (!string.IsNullOrEmpty(limId2))
                    {
                        var limSet2 = _connection.CreateCommandAndParameters("/tool/user-manager/profile/limit/set",
                            TikSpecialProperties.Id, limId2,
                            "transfer-limit", transferLimit);
                        limSet2.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void DeleteUserManagerProfile(string name)
        {
            if (_connection == null || !_connection.IsOpened) throw new InvalidOperationException("Not connected");
            try
            {
                var find = _connection.CreateCommandAndParameters("/userman/profile/print", "name", name);
                var id = find.ExecuteList().FirstOrDefault()?.GetId();
                if (!string.IsNullOrEmpty(id))
                {
                    var del = _connection.CreateCommandAndParameters("/userman/profile/remove", TikSpecialProperties.Id, id);
                    del.ExecuteNonQuery();
                }
                var limFind = _connection.CreateCommandAndParameters("/userman/limitation/print", "profile", name);
                var limId = limFind.ExecuteList().FirstOrDefault()?.GetId();
                if (!string.IsNullOrEmpty(limId))
                {
                    var limDel = _connection.CreateCommandAndParameters("/userman/limitation/remove", TikSpecialProperties.Id, limId);
                    limDel.ExecuteNonQuery();
                }
            }
            catch
            {
                var find2 = _connection.CreateCommandAndParameters("/tool/user-manager/profile/print", "name", name);
                var id2 = find2.ExecuteList().FirstOrDefault()?.GetId();
                if (!string.IsNullOrEmpty(id2))
                {
                    var del2 = _connection.CreateCommandAndParameters("/tool/user-manager/profile/remove", TikSpecialProperties.Id, id2);
                    del2.ExecuteNonQuery();
                }
                var limFind2 = _connection.CreateCommandAndParameters("/tool/user-manager/profile/limit/print", "profile", name);
                var limId2 = limFind2.ExecuteList().FirstOrDefault()?.GetId();
                if (!string.IsNullOrEmpty(limId2))
                {
                    var limDel2 = _connection.CreateCommandAndParameters("/tool/user-manager/profile/limit/remove", TikSpecialProperties.Id, limId2);
                    limDel2.ExecuteNonQuery();
                }
            }
        }

        public class UnifiedProfileModel
        {
            public UserManagerProfile Profile { get; set; } = new UserManagerProfile();
            public List<ProfileLimit> Limits { get; set; } = new List<ProfileLimit>();
            public List<UserProfileAssignment> Assignments { get; set; } = new List<UserProfileAssignment>();
        }

        public static List<UnifiedProfileModel> BuildUnifiedProfiles()
        {
            var profiles = GetUserManagerProfiles();
            var limits = GetProfileLimits();
            var assigns = GetUserProfileAssignments();
            var list = new List<UnifiedProfileModel>();
            foreach (var p in profiles)
            {
                var m = new UnifiedProfileModel
                {
                    Profile = p,
                    Limits = limits.Where(l => string.Equals(l.Profile, p.Name, StringComparison.OrdinalIgnoreCase)).ToList(),
                    Assignments = assigns.Where(a => !string.IsNullOrEmpty(a.Profile) && string.Equals(a.Profile, p.Name, StringComparison.OrdinalIgnoreCase)).ToList()
                };
                list.Add(m);
            }
            return list;
        }

        public class UmUser
        {
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
            public string Profile { get; set; } = "";
            public string UptimeUsed { get; set; } = "";
            public string DownloadUsed { get; set; } = "";
            public string UploadUsed { get; set; } = "";
            public string Comment { get; set; } = "";
            public string Status { get; set; } = "";
        }

        public static List<UmUser> GetUserManagerUsers()
        {
            try
            {
                if (_connection == null || !_connection.IsOpened) return new List<UmUser>();
                var cmd = _connection.CreateCommand("/userman/user/print");
                var rows = cmd.ExecuteList();
                if (rows.Count() == 0)
                {
                    cmd = _connection.CreateCommand("/tool/user-manager/user/print");
                    rows = cmd.ExecuteList();
                }
                var list = new List<UmUser>();
                foreach (var r in rows)
                {
                    list.Add(new UmUser
                    {
                        Username = r.GetResponseField("username"),
                        Password = r.GetResponseField("password"),
                        Profile = r.GetResponseField("actual-profile") ?? r.GetResponseField("profile"),
                        UptimeUsed = r.GetResponseField("uptime-used"),
                        DownloadUsed = r.GetResponseField("downloaded"),
                        UploadUsed = r.GetResponseField("uploaded"),
                        Comment = r.GetResponseField("comment"),
                        Status = r.GetResponseField("status"),
                    });
                }
                return list;
            }
            catch { return new List<UmUser>(); }
        }

        public static void AddUserManagerUserAdvanced(string username, string password, string profile, string timeLimit, string rateLimit, string comment)
        {
            if (_connection == null || !_connection.IsOpened) throw new InvalidOperationException("Not connected");
            try
            {
                var add = _connection.CreateCommandAndParameters("/userman/user/add",
                    "username", username,
                    "password", password,
                    "actual-profile", profile,
                    "comment", comment,
                    "time-limit", timeLimit,
                    "rate-limit", rateLimit);
                add.ExecuteNonQuery();
            }
            catch
            {
                var add2 = _connection.CreateCommandAndParameters("/tool/user-manager/user/add",
                    "username", username,
                    "password", password,
                    "profile", profile,
                    "comment", comment,
                    "time-limit", timeLimit,
                    "rate-limit", rateLimit);
                add2.ExecuteNonQuery();
            }
        }

        public static void UpdateUserManagerUser(string username, string password, string profile, string comment)
        {
            if (_connection == null || !_connection.IsOpened) throw new InvalidOperationException("Not connected");
            var find = _connection.CreateCommandAndParameters("/userman/user/print", "username", username);
            var row = find.ExecuteList();
            var id = row.FirstOrDefault()?.GetId();
            if (string.IsNullOrEmpty(id)) return;
            var set = _connection.CreateCommandAndParameters("/userman/user/set",
                TikSpecialProperties.Id, id,
                "password", password,
                "comment", comment,
                "actual-profile", profile);
            set.ExecuteNonQuery();
        }

        public static void DeleteUserManagerUser(string username)
        {
            if (_connection == null || !_connection.IsOpened) throw new InvalidOperationException("Not connected");
            var find = _connection.CreateCommandAndParameters("/userman/user/print", "username", username);
            var row = find.ExecuteList();
            var id = row.FirstOrDefault()?.GetId();
            if (string.IsNullOrEmpty(id)) return;
            var del = _connection.CreateCommandAndParameters("/userman/user/remove", TikSpecialProperties.Id, id);
            del.ExecuteNonQuery();
        }

        public class UmSession
        {
            public string Username { get; set; } = "";
            public string MacAddress { get; set; } = "";
            public string DownloadUsed { get; set; } = "";
            public string UploadUsed { get; set; } = "";
            public string UptimeUsed { get; set; } = "";
            public string Active { get; set; } = "";
        }

        public static List<UmSession> GetUserManagerSessions()
        {
            try
            {
                if (_connection == null || !_connection.IsOpened) return new List<UmSession>();
                var cmd = _connection.CreateCommand("/userman/session/print");
                var rows = cmd.ExecuteList();
                if (rows.Count() == 0)
                {
                    cmd = _connection.CreateCommand("/tool/user-manager/session/print");
                    rows = cmd.ExecuteList();
                }
                var list = new List<UmSession>();
                foreach (var r in rows)
                {
                    list.Add(new UmSession
                    {
                        Username = r.GetResponseField("user") ?? r.GetResponseField("username"),
                        MacAddress = r.GetResponseField("mac-address"),
                        DownloadUsed = r.GetResponseField("downloaded"),
                        UploadUsed = r.GetResponseField("uploaded"),
                        UptimeUsed = r.GetResponseField("uptime") ?? r.GetResponseField("uptime-used"),
                        Active = r.GetResponseField("active") ?? r.GetResponseField("status")
                    });
                }
                return list;
            }
            catch { return new List<UmSession>(); }
        }

        public static ITikConnection Connection => _connection;
    }
}
