using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.Algorithms.ConnectPool;

/// <summary>
/// 一个连接池管理器，负责维护多个可用的连接，并且自动清理，扩容，用于快速读写服务器或是PLC时使用。<br />
/// A connection pool manager is responsible for maintaining multiple available connections, 
/// and automatically cleans up, expands, and is used to quickly read and write servers or PLCs.
/// </summary>
/// <typeparam name="TConnector">管理的连接类，需要支持IConnector接口</typeparam>
/// <remarks>
/// 需要先实现 <see cref="IConnector" /> 接口的对象，然后就可以实现真正的连接池了，理论上可以实现任意的连接对象，包括modbus连接对象，各种PLC连接对象，数据库连接对象，redis连接对象，SimplifyNet连接对象等等。下面的示例就是modbus-tcp的实现
/// <note type="warning">要想真正的支持连接池访问，还需要服务器支持一个端口的多连接操作，三菱PLC的端口就不支持，如果要测试示例代码的连接池对象，需要使用本组件的<see cref="T:HslCommunication.ModBus.ModbusTcpServer" />来创建服务器对象</note>
/// </remarks>
public class ConnectPool<TConnector> where TConnector : IConnector
{
    private Func<TConnector> CreateConnector = null;

    private int maxConnector = 10;

    private int usedConnector = 0;

    private int usedConnectorMax = 0;

    private int expireTime = 30;

    private bool canGetConnector = true;

    private Timer timerCheck = null;

    private object listLock;

    private List<TConnector> connectors = null;

    /// <summary>
    /// 获取或设置最大的连接数，当实际的连接数超过最大的连接数的时候，就会进行阻塞，直到有新的连接对象为止。<br />
    /// Get or set the maximum number of connections. When the actual number of connections exceeds the maximum number of connections, 
    /// it will block until there is a new connection object.
    /// </summary>
    public int MaxConnector
    {
        get
        {
            return maxConnector;
        }
        set
        {
            maxConnector = value;
        }
    }

    /// <summary>
    /// 获取或设置当前连接过期的时间，单位秒，默认30秒，也就是说，当前的连接在设置的时间段内未被使用，就进行释放连接，减少内存消耗。<br />
    /// Get or set the expiration time of the current connection, in seconds, the default is 30 seconds, that is, 
    /// if the current connection is not used within the set time period, the connection will be released to reduce memory consumption.
    /// </summary>
    public int ConectionExpireTime
    {
        get
        {
            return expireTime;
        }
        set
        {
            expireTime = value;
        }
    }

    /// <summary>
    /// 当前已经使用的连接数，会根据使用的频繁程度进行动态的变化。<br />
    /// The number of currently used connections will dynamically change according to the frequency of use.
    /// </summary>
    public int UsedConnector => usedConnector;

    /// <summary>
    /// 当前已经使用的连接数的峰值，可以用来衡量当前系统的适用的连接池上限。<br />
    /// The current peak value of the number of connections used can be used to measure the upper limit of the applicable connection pool of the current system.
    /// </summary>
    public int UseConnectorMax => usedConnectorMax;

    /// <summary>
    /// 实例化一个连接池对象，需要指定如果创建新实例的方法<br />
    /// To instantiate a connection pool object, you need to specify how to create a new instance
    /// </summary>
    /// <param name="createConnector">创建连接对象的委托</param>
    public ConnectPool(Func<TConnector> createConnector)
    {
        CreateConnector = createConnector;
        listLock = new object();
        connectors = new List<TConnector>();
        timerCheck = new Timer(TimerCheckBackground, null, 10000, 30000);
    }

    /// <summary>
    /// 获取一个可用的连接对象，如果已经达到上限，就进行阻塞等待。当使用完连接对象的时候，需要调用<see cref="M:HslCommunication.Algorithms.ConnectPool.ConnectPool`1.ReturnConnector(`0)" />方法归还连接对象。<br />
    /// Get an available connection object, if the upper limit has been reached, block waiting. When the connection object is used up, 
    /// you need to call the <see cref="M:HslCommunication.Algorithms.ConnectPool.ConnectPool`1.ReturnConnector(`0)" /> method to return the connection object.
    /// </summary>
    /// <returns>可用的连接对象</returns>
    public TConnector GetAvailableConnector()
    {
        while (!canGetConnector)
        {
            CommHelper.ThreadSleep(20);
        }
        var val = default(TConnector);
        lock (listLock)
        {
            TConnector val2;
            for (var i = 0; i < connectors.Count; i++)
            {
                val2 = connectors[i];
                if (!val2.IsConnectUsing)
                {
                    val2 = connectors[i];
                    val2.IsConnectUsing = true;
                    val = connectors[i];
                    break;
                }
            }
            if (val == null)
            {
                val = CreateConnector();
                val.IsConnectUsing = true;
                ref TConnector reference = ref val;
                val2 = default;
                if (val2 == null)
                {
                    val2 = reference;
                    reference = ref val2;
                }
                reference.LastUseTime = DateTime.Now;
                val.Open();
                connectors.Add(val);
                usedConnector = connectors.Count;
                if (usedConnector > usedConnectorMax)
                {
                    usedConnectorMax = usedConnector;
                }
                if (usedConnector == maxConnector)
                {
                    canGetConnector = false;
                }
            }
            ref TConnector reference2 = ref val;
            val2 = default;
            if (val2 == null)
            {
                val2 = reference2;
                reference2 = ref val2;
            }
            reference2.LastUseTime = DateTime.Now;
        }
        return val;
    }

    /// <summary>
    /// 使用完之后需要通知连接池的管理器，本方法调用之前需要获取到连接对象信息。<br />
    /// After using it, you need to notify the manager of the connection pool, and you need to get the connection object information before calling this method.
    /// </summary>
    /// <param name="connector">连接对象</param>
    public void ReturnConnector(TConnector connector)
    {
        lock (listLock)
        {
            var num = connectors.IndexOf(connector);
            if (num != -1)
            {
                var val = connectors[num];
                val.IsConnectUsing = false;
            }
        }
    }

    /// <summary>
    /// 将目前连接中的所有对象进行关闭，然后移除队列。<br />
    /// Close all objects in the current connection, and then remove the queue.
    /// </summary>
    public void ResetAllConnector()
    {
        lock (listLock)
        {
            for (var num = connectors.Count - 1; num >= 0; num--)
            {
                connectors[num].Close();
                connectors.RemoveAt(num);
            }
        }
    }

    private void TimerCheckBackground(object obj)
    {
        lock (listLock)
        {
            for (var num = connectors.Count - 1; num >= 0; num--)
            {
                if ((DateTime.Now - connectors[num].LastUseTime).TotalSeconds > expireTime && !connectors[num].IsConnectUsing)
                {
                    connectors[num].Close();
                    connectors.RemoveAt(num);
                }
            }
            usedConnector = connectors.Count;
            if (usedConnector < MaxConnector)
            {
                canGetConnector = true;
            }
        }
    }
}
