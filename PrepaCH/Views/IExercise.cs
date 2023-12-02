using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrepaCH.Views
{
    internal interface IExercise
    {
        public void Setup(List<Rank> ranks);
        public void Reset();
        public Task<bool> Check();
    }
}
