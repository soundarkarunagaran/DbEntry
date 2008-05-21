﻿using System;
using System.Web.UI.Design;

namespace Lephone.Web.Common
{
    public class DbEntryDataSourceFieldSchema : IDataSourceFieldSchema
    {
        public DbEntryDataSourceFieldSchema(string Name, Type DataType,
            bool Identity, bool PrimaryKey, bool Nullable)
        {
            _Name = Name;
            _DataType = DataType;
            _Identity = Identity;
            _PrimaryKey = PrimaryKey;
            _Nullable = Nullable;
        }

        private readonly Type _DataType;

        public Type DataType
        {
            get { return _DataType; }
        }

        private readonly bool _Identity;

        public bool Identity
        {
            get { return _Identity; }
        }

        private readonly string _Name;

        public string Name
        {
            get { return _Name; }
        }

        private readonly bool _PrimaryKey;

        public bool PrimaryKey
        {
            get { return _PrimaryKey; }
        }

        public bool IsReadOnly
        {
            get { return _Identity; }
        }

        public bool IsUnique
        {
            get { return _PrimaryKey || _Identity; }
        }

        public int Length
        {
            get { return 0; }
        }

        private readonly bool _Nullable;

        public bool Nullable
        {
            get { return _Nullable; }
        }

        public int Precision
        {
            get { return 0; }
        }

        public int Scale
        {
            get { return 0; }
        }
    }
}