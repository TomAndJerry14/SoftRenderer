using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace SoftRenderer
{
    public class Object
    {
        public Vector3[] vertices;
        public Vector3[] texture_vertices;
        public Vector3[] normals;
        public int[][] face_vertices;
        public int[][] face_texture_vertices;
        public int[][] face_normal_vertices;
        public string group;
        public int smooth;

        public Object(string text)
        {
            //Regex r = new Regex(@"v (-?[0-9]*\.?[0-9]*(e-\d+)?) (-?[0-9]*\.?[0-9]*(e-\d+)?) (-?[0-9]*\.?[0-9]*(e-\d+)?)");
            //var mc = r.Matches(text);

            //vertices = new Vector3[mc.Count];
            //int index = 0;
            //foreach (string item in mc)
            //{

            //}

            //r = new Regex(@"vt (-?[0-9]*\.?[0-9]*(e-\d+)?) (-?[0-9]*\.?[0-9]*(e-\d+)?) (-?[0-9]*\.?[0-9]*(e-\d+)?)");
            //mc = r.Matches(text);
            //vertices = new Vector3[mc.Count];
            //index = 0;
            //foreach (string item in mc)
            //{
            //    Vector3 point = ParseTextureVertex(item);
            //    vertices[index] = point;
            //    index++;
            //}

        }

        public Object(string[] lines)
        {
            int index = 0;
            List<Vector3> _vertices = new List<Vector3>();
            List<Vector3> _texture_vertices = new List<Vector3>();
            List<Vector3> _normals = new List<Vector3>();
            List<int[]> _face_vertices = new List<int[]>();
            List<int[]> _face_texture_vertices = new List<int[]>();
            List<int[]> _face_normal_vertices = new List<int[]>();
            string group;
            int smooth;
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (lines[i].StartsWith("vt"))
                {
                    Vector3 point = ParseTextureVertex(line);
                    _texture_vertices.Add(point);
                }
                else if (lines[i].StartsWith("f"))
                {
                    int[,] face = ParseFaces(line);

                    int[] face_v = new int[3];
                    int[] face_vt = new int[3];
                    int[] face_vn = new int[3];
                    for (int j = 0; j < 3; j++)
                    {
                        face_v[j] = face[j, 0];
                        face_vt[j] = face[j, 1];
                        face_vn[j] = face[j, 2];
                    }
                    _face_vertices.Add(face_v);
                    _face_texture_vertices.Add(face_vt);
                    _face_normal_vertices.Add(face_vn);

                }
                else if (lines[i].StartsWith("vn"))
                {
                    Vector3 point = ParseVertex(line);
                    _normals.Add(point);
                }
                else if (lines[i].StartsWith("v"))
                {
                    Vector3 point = ParseVertex(line);
                    index++;
                    _vertices.Add(point);
                }
                else if (lines[i].StartsWith("g"))
                {
                }
                else if (lines[i].StartsWith("o"))
                {
                }
                else if (lines[i].StartsWith("mtllib"))
                {
                }
                else if (lines[i].StartsWith("usemtl"))
                {
                }
                else if (lines[i].StartsWith("newmtl"))
                {
                }
                else if (lines[i].StartsWith("Ka"))
                {
                }
                else if (lines[i].StartsWith("Kd"))
                {
                }
                else if (lines[i].StartsWith("Ks"))
                {
                }
                else if (lines[i].StartsWith("d"))
                {
                }
                else if (lines[i].StartsWith("Tr"))
                {
                }
                else if (lines[i].StartsWith("map_Ka"))
                {
                }
                else if (lines[i].StartsWith("map_Kd"))
                {
                }
            }

            vertices = _vertices.ToArray();
            texture_vertices = _texture_vertices.ToArray();
            normals = _normals.ToArray();
            face_vertices = _face_vertices.ToArray();
            face_texture_vertices = _face_texture_vertices.ToArray();
            face_normal_vertices = _face_normal_vertices.ToArray();
        }

        Vector3 ParseVertex(string oneLine)
        {
            Regex regex = new Regex(@"(-?[0-9]+\.?[0-9]*(e-\d+)?)");
            var mc = regex.Matches(oneLine);
            Vector3 point = new Vector3();
            point.x = float.Parse(mc[0].Value);
            point.y = float.Parse(mc[1].Value);
            point.z = float.Parse(mc[2].Value);
            return point;
        }

        Vector3 ParseTextureVertex(string oneLine)
        {
            Regex regex = new Regex(@"(-?[0-9]+\.?[0-9]*(e-\d+)?)");
            var mc = regex.Matches(oneLine);
            Vector3 point = new Vector3();
            point.x = float.Parse(mc[0].Value);
            point.y = float.Parse(mc[1].Value);
            point.z = float.Parse(mc[2].Value);
            return point;
        }

        int[,] ParseFaces(string oneLine)
        {
            int[,] face = new int[3, 3];
            Regex regex = new Regex(@"([0-9]+/[0-9]+/[0-9]+)");
            var mc = regex.Matches(oneLine);

            for (int i = 0; i < 3; i++)
            {
                var _splits = mc[i].Value.Split('/');
                for (int j = 0; j < 3; j++)
                {
                    face[i, j] = int.Parse(_splits[j]);
                }
            }

            return face;
        }
    }
}
