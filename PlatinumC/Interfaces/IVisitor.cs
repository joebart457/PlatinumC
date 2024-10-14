using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Interfaces
{
    public interface IVisitable<Ty>
    {
        public void Visit(Ty t);
    }

    public interface IVisitable<Ty, TyResult>
    {
        public TyResult Visit(Ty t);
    }

    public interface IVisitable<Ty, Ty2, TyResult>
    {
        public TyResult Visit(Ty t, Ty2 t2);
    }
}
