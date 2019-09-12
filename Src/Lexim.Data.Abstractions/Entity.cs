using System;

namespace Lexim.Data
{
    public abstract class Entity
    {
        public virtual int Id { get; set; }
        public virtual DateTime CreatedOn { get; set; }
        public virtual DateTime? ModifiedOn { get; set; }
    }
}