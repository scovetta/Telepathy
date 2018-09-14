using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Xml;

using Microsoft.Hpc.Scheduler.Properties;
using Microsoft.Hpc.Scheduler.Store;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para />
    /// </summary>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidSchedulerPoolClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public class SchedulerPool : SchedulerObjectBase, ISchedulerPool
    {
        ISchedulerStore _store = null;
        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        protected Dictionary<PropertyId, StoreProperty> _changeProps = new Dictionary<PropertyId, StoreProperty>();
        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        protected IClusterPool _pool = null;

        internal protected SchedulerPool(ISchedulerStore store)
        {
            _store = store;
        }

        /// <summary>
        ///   <para />
        /// </summary>
        internal protected SchedulerPool()
        {
        }

        internal void Init(IClusterPool pool)
        {
            _pool = pool;

            InitFromObject(_pool, null);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        internal protected override PropertyId[] GetPropertyIds()
        {
            PropertyId[] pids =
            {
                PoolPropertyIds.Id,
                PoolPropertyIds.Name,
                PoolPropertyIds.Weight,
                PoolPropertyIds.Guarantee,
                PoolPropertyIds.CurrentAllocation
            };
            return pids;
        }


        #region ISchedulerPool Members

        /// <summary>
        ///   <para />
        /// </summary>
        public void Refresh()
        {
            _changeProps.Clear();

            InitFromObject(_pool, null);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        public void Commit()
        {
            if (_pool == null)
            {
                //TODO: localize this
                throw new SchedulerException("Pool not yet created");
            }
            if (_changeProps.Keys.Count > 0)
            {
                _pool.SetProps(new List<StoreProperty>(_changeProps.Values).ToArray());
                _changeProps.Clear();
            }
        }


        System.String _Name_Default = "";
        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public string Name
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(PoolPropertyIds.Name, out prop))
                {
                    return (System.String)prop.Value;
                }
                return _Name_Default;
            }
        }

        System.Int32 _Id_Default = -1;
        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int Id
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(PoolPropertyIds.Id, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                return _Id_Default;
            }
        }

        System.Int32 _Weight_Default = 0;
        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int Weight
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(PoolPropertyIds.Weight, out prop))
                {
                    return (System.Int32)prop.Value;
                }
                return _Weight_Default;
            }
            set
            {
                StoreProperty prop = new StoreProperty(PoolPropertyIds.Weight, value);
                _props[PoolPropertyIds.Weight] = prop;
                _changeProps[PoolPropertyIds.Weight] = prop;
            }
        }


        System.Int32 _Guarantee_Default = 0;
        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int Guarantee
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(PoolPropertyIds.Guarantee, out prop))
                {
                    return (System.Int32)prop.Value;
                }
                return _Guarantee_Default;
            }
        }

        System.Int32 _CurrentAllocation_Default = 0;
        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int CurrentAllocation
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(PoolPropertyIds.CurrentAllocation, out prop))
                {
                    return (System.Int32)prop.Value;
                }
                return _CurrentAllocation_Default;
            }
        }


        #endregion
    }
}