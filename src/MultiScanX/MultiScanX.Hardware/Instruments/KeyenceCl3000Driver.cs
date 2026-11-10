using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MultiScanX.Hardware.Instruments
{
    /// <summary>
    /// Keyence CL-3000 초정밀 공초점 컨트롤러 Ethernet TCP/IP 통신 드라이버
    /// (센서 헤드: CL-S015 고정밀 다파장 공초점 변위센서)
    /// </summary>
    public class KeyenceCl3000Driver
    {
        private TcpClient? _client;
        private NetworkStream? _stream;
        private readonly string _ipAddress;
        private readonly int _port;

        public bool IsConnected => _client != null && _client.Connected;

        public KeyenceCl3000Driver(string ipAddress = "192.168.0.100", int port = 24685)
        {
            _ipAddress = ipAddress;
            _port = port;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(_ipAddress, _port);
                _stream = _client.GetStream();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Disconnect()
        {
            _stream?.Close();
            _client?.Close();
            _stream = null;
            _client = null;
        }

        /// <summary>
        /// 현재 계측값(OUT1 ~ OUT4) 단발 읽기 명령 (M0 커맨드)
        /// 반환값: 단위 mm (CL-S015 측정치)
        /// </summary>
        public async Task<double> ReadMeasurementAsync(int outChannel = 1)
        {
            if (!IsConnected || _stream == null) return 0.0;

            try
            {
                // Keyence CL-3000 단발 측정 커맨드: "M0\r\n"
                byte[] cmd = Encoding.ASCII.GetBytes("M0\r\n");
                await _stream.WriteAsync(cmd, 0, cmd.Length);

                byte[] buffer = new byte[256];
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();

                // 응답 예시: "M0,+00.123456\r"
                string[] parts = response.Split(',');
                if (parts.Length >= 2 && double.TryParse(parts[outChannel], out double val))
                {
                    return val; // mm 단위 변위값
                }
            }
            catch (Exception)
            {
                // 통신 타임아웃 또는 예외 처리
            }

            return 0.0;
        }
    }
}

