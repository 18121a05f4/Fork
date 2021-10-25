﻿using Azylee.Core.AppUtils.AppConfigUtils;
using Azylee.Core.AppUtils.AppConfigUtils.AppConfigInterfaces;
using Azylee.Core.AppUtils.AppConfigUtils.AppConfigModels;
using Azylee.Core.DataUtils.DateTimeUtils;
using Azylee.Core.IOUtils.FileUtils;
using Azylee.Core.IOUtils.TxtUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Azylee.Jsons.JsonAppConfigUtils
{
    /// <summary>
    /// Json 配置管理器
    /// 
    /// 如何使用：
    /// 1. 根据自己需求，创建配置类，实现IAppConfigModel接口
    /// 2. 使用配置管理器，new出创建的配置类
    /// 3. 通过配置管理器，管理配置信息即可
    /// PS. 建议直接使用静态变量创建
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonAppConfig<T> : AppConfig<T> where T : IAppConfigModel, new()
    {
        private string FilePath;
        private string FilePathBackup;

        private JsonAppConfig() { }
        /// <summary>
        /// 构造配置管理器
        /// </summary>
        /// <param name="filepath">配置文件路径</param>
        public JsonAppConfig(string filepath)
        {
            this.FilePath = filepath;
            this.FilePathBackup = filepath + ".backup";

            // 配置初始化
            OnCreate();
            // 重置配置项
            this.Config.ForceSet();
            // 保存配置
            DoSave();
        }
        public override bool OnCreate()
        {
            // 读取默认配置文件
            if (File.Exists(this.FilePath))
            {
                this.Config = Json.File2Object<T>(this.FilePath);
            }
            // 读取备份的配置文件
            if (this.Config == null)
            {
                if (File.Exists(this.FilePathBackup))
                {
                    this.Config = Json.File2Object<T>(this.FilePathBackup);
                }
            }

            if (this.Config == null)
            {
                // 配置读取为空，有可能是配置内容发生结构性改变，JSON解析失败
                // 所以，此处对原配置文件先进行备份，防止内容丢失
                FileTool.Copy(this.FilePath, this.FilePath + ".backup" + "." + DateTimeFormatter.Compact(DateTime.Now), true);

                this.Config = new T();
            }
            return true;
        }

        public override bool OnDestroy()
        {
            return DoSave();
        }
        /// <summary>
        /// 保存配置信息
        /// </summary>
        /// <returns></returns> 
        public override bool DoSave()
        {
            string s = Json.Object2String(this.Config);
            s = JsonFormat.Format(s);

            TxtTool.Create(this.FilePath, s);
            bool result = TxtTool.Create(this.FilePathBackup, s);

            if (result)
            {
                if (Json.File2Object<T>(this.FilePathBackup) != null)
                {
                    if (FileTool.Copy(this.FilePathBackup, this.FilePath, true))
                    {
                        FileTool.Delete(this.FilePathBackup);
                    }
                }
            }
            return result;
        }
    }
}
