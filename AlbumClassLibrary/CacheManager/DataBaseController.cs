﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlbumClassLibrary.CacheManager
{
    /// <summary>
    /// Класс взаимодействия с файловой БД
    /// </summary>
    internal class DataBaseController : ControllerBase.DataBaseControllerBase, IDisposable
    {
        public DataBaseController(string dataBaseFilePath) : base(dataBaseFilePath) { }

        /// <summary>
        /// Поиск файла в кеше
        /// </summary>
        /// <param name="path">Путь к оригиналу файла</param>
        /// <param name="bytes">Исходящий массив байтов в случае успешного поиска</param>
        /// <returns>Успешность поиска</returns>
        public bool CacheSeach(string path, out byte[] bytes)
        {
            string sql = $"SELECT imageBinary FROM cache WHERE filePath = '{path}' ";

            try
            {
                m_dbConnection.Open();

                System.Data.DataTable table = new System.Data.DataTable();

                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, m_dbConnection);
                adapter.Fill(table);

                if(table.Rows.Count == 0)
                {
                    bytes = null;
                    return false;
                }

                bytes = (from row in table.Rows.Cast<System.Data.DataRow>()
                         select (byte[])row[0]).Single();

                if (bytes != null)
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                bytes = null;
                return false;
                throw;
            }
            finally
            {
                m_dbConnection.Close();
            }
        }

        public bool CacheSeach(string path, out System.Windows.Size size)
        {
            string sql = $"SELECT width, height FROM cache WHERE filePath = '{path}' ";

            try
            {
                m_dbConnection.Open();

                System.Data.DataTable table = new System.Data.DataTable();

                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, m_dbConnection);
                adapter.Fill(table);

                if (table.Rows.Count == 0)
                {
                    size = new System.Windows.Size();
                    return false;
                }
                size = (from row in table.Rows.Cast<System.Data.DataRow>()
                         select new System.Windows.Size(Int32.Parse(row[0].ToString()), Int32.Parse(row[1].ToString()))).Single();

                if (size != new System.Windows.Size())
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                size = new System.Windows.Size();
                return false;
                throw;
            }
            finally
            {
                m_dbConnection.Close();
            }
        }

        /// <summary>
        /// Добавить новую запись в кеш
        /// </summary>
        /// <param name="path">Путь к оригиналу файла</param>
        /// <param name="img">Ужатое изображение</param>
        public void CacheAdd(string path, byte[] img)
        {
            SQLiteParameter param = new SQLiteParameter("@image", DbType.Binary)
            {
                Value = img
            };

            string sql = $"INSERT OR IGNORE INTO cache (filePath, imageBinary, addedDate) values ('{path}', @image, '{DateTime.Now.ToFileTimeUtc()}'); UPDATE cache  SET imageBinary = @image, addedDate = '{DateTime.Now.ToFileTimeUtc()}' WHERE filePath LIKE '{path}'; ";

            ExecuteNonQuery(sql, param);
        }

        public void SizeAdd(string path, System.Windows.Size size)
        {
            string sql = $"INSERT OR IGNORE INTO cache (filePath, addedDate, width, height) VALUES ('{path}', '{DateTime.Now.ToFileTimeUtc()}', '{size.Width}', '{size.Height}'); UPDATE cache  SET width = '{size.Width}', height = '{size.Height}' WHERE filePath LIKE '{path}'; ";

            ExecuteNonQuery(sql);
        }

        #region IDisposable Support
        private bool disposedValue = false; // Для определения избыточных вызовов

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    base.m_dbConnection.Close();
                    m_dbConnection = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
