using System.Net;
using System.Xml.Linq;
using LINQPad.Extensibility.DataContext;

namespace DataContextDriverDemo.Astoria
{
    /// <summary>
    /// Wrapper to expose typed properties over ConnectionInfo.DriverData.
    /// </summary>
    class ConnectionProperties
    {
        readonly IConnectionInfo _cxInfo;
        readonly XElement _driverData;

        public ConnectionProperties (IConnectionInfo cxInfo)
        {
            _cxInfo = cxInfo;
            _driverData = cxInfo.DriverData;
        }

        public bool Persist
        {
            get => _cxInfo.Persist;
            set => _cxInfo.Persist = value;
        }

        public string ProcessName
        {
            get => (string)_driverData.Element (nameof(ProcessName)) ?? "";
            set => _driverData.SetElementValue (nameof(ProcessName), value);
        }
    }
}
