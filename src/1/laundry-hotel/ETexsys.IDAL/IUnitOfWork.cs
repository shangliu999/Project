using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.IDAL
{
    /// <summary>
    /// 业务单元操作接口
    /// </summary>
    public interface IUnitOfWork
    {
        #region 属性

        /// <summary>
        /// 获取 当前操作单元是否被提交
        /// </summary>
        bool IsCommitted { get; }

        #endregion

        #region 方法

        int Commit();

        void Rollback();

        #endregion
    }
}
