using SoftRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoftRenderer
{
    public class Light
    {
        public float intensity { get; set; } = 10f;
        public Vector3 rot { get; set; } = new Vector3(100, -10, 10);

        public Vector3 dir { get; } = -Vector3.forward;
    }
}
