﻿using AlbumClassLibrary.AlbumManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlbumClassLibrary
{
    public class SystemAlbum : IAlbum
    {
        public string AlbumName => "Системный альбом";
        public Guid AlbumTypeGuid => Guid.Parse("804C01FD-772B-48E0-916C-1C24FC99968E");

        private int Id { get; }
        public string DisplayName { get; set; }

        public List<IFolder> Folders { get; set; }

        public Guid AlbumGuid { get; set; }

        private bool _isCurrent;
        public bool IsCurrent
        {
            get
            {
                return _isCurrent;
            }
            set
            {
                _isCurrent = value;
                PriorityChanged(this, new EventArgs());
            }
        }

        public IAlbum FromMapper(IAlbum album)
        {
            if (album.AlbumTypeGuid == this.AlbumTypeGuid)
            {
                Folders = album.Folders;
                AlbumGuid = album.AlbumGuid;
                DisplayName = album.DisplayName;
                Folders = album.Folders;
            }
            else
                throw new InvalidCastException();

            return this;
        }

        public event EventHandler FolderAdded;
        public event EventHandler PriorityChanged;

        /// <summary>
        /// Реализация этого метода на плечах пользователя
        /// </summary>
        /// <remarks>
        /// Рекомендуемая реализация этого метода:
        /// 1) Вызываем форму из которой получим папку, в которой находятся (или будут находиться) изображения
        /// 2.1) Если это парсер - наполняем папку изображениями (Откуда и как - решаем по данным формы из 1 пункта)
        /// 2.2) Если это просто папка - ничего не делаем
        /// 3) Из папки по факту получаем список файлов и всю эту кучу дружно сейвим как новую папку с файлами в этом альбоме
        /// </remarks>
        public void AddFolder()
        {
            if (Folders == null)
                Folders = new List<IFolder>();

            // 1 - форма. Для системного альбома - это форма выбора папки из файловой системы
            var folderToLoad = ShowForm();

            // 2 - пропускаем, наполнение папки не требуется

            // 3 - Добавляем в текущий альбом папку
            IFolder folder = new Folder(0, this.AlbumGuid.ToString(), $"{DateTime.Now.ToString()}", folderToLoad, null);
            Folders.Add(folder);
            // 4 - сохраняем все в БД через событие FolderAdded(IFolder, new EventArgs())
            FolderAdded(folder, new EventArgs());
        }

        private string ShowForm()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    return dialog.SelectedPath;
                }
                else
                    return null;
            }
        }
    }
}