using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PartyMemberManager.Dal.Entities
{
    /// <summary>
    /// 实体类基类
    /// </summary>
    [DataContract]
    public class EntityBase
    {
        public EntityBase()
        {
            Id = Guid.NewGuid();
            CreateTime = DateTime.Now;
            IsDeleted = false;
            Ordinal = 0;
        }
        /// <summary>
        /// Id
        /// </summary>
        [DisplayName("Id")]
        [DataMember]
        public Guid Id { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [DisplayName("创建时间")]
        [DataType(DataType.DateTime)]
        [DataMember]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 操作员Id(创建人)
        /// </summary>
        [DisplayName("操作员Id")]
        [JsonIgnore]
        [IgnoreDataMember]
        public Guid? OperatorId { get; set; }
        /// <summary>
        /// 显示顺序
        /// </summary>
        [DisplayName("显示顺序")]
        [DataMember]
        public int Ordinal { get; set; }
        /// <summary>
        /// 删除标志(只有管理员可以删除)
        /// </summary>
        [DisplayName("删除标志")]
        [DataMember]
        public bool IsDeleted { get; set; }
    }
}
