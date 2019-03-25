using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using GeoFotos.Base;
using GeoFotos.Models;
namespace GeoFotos.Bussines
{
    public class ConsultBackupBussiness
    {
        #region dataMigration  
        public ImageTotal GetListImageByKeyUri(string key, string uri)
        {
            ImageTotal _imageData = new ImageTotal();
            IList<ContextImageData> _contextImageData = new List<ContextImageData>();
            var _modelParameters = CheckExist(key, uri);
            if (_modelParameters.Count > 0)
            {
                //Migrar
                var result = MigracionImage(_modelParameters, key, uri);
                _imageData = GetImageData(key, uri);
            }
            else
            {

                //recuperar
                _imageData = GetImageData(key, uri);
                if (_imageData != null)
                {
                    _imageData = MigrationNull(uri, key);
                }
            }
            return _imageData;
        }

        public IList<ConfigParametersImagesAzure> CheckExist(string key, string uri)
        {
            using (var tran = new MardisEngine_TestEntities())
            {
                var sentenciaSQL = $@"EXEC dbo.sp_Config_Parameters_Images_Azure @uri ='{uri}',@key = '{key}'";
                var resultProc = tran.Database.SqlQuery<ConfigParametersImagesAzure>(sentenciaSQL).ToList();

                return resultProc;
            }
        }

        public ImageTotal GetImageData(string key, string uri)
        {
            using (var tran = new MardisEngine_TestEntities())
            {

                var sentenciaSQL = tran.backup_images_project
                    .Where(h => h.project == key && h.id_core == uri)
                    .Select(x => new ImageData
                    {
                        uri = x.id_core,
                        Code = x.project,
                        link = x.link_azure,
                        nameImg = x.filename,
                        name = x.name
                    })
                    .ToList<ImageData>();
                ImageTotal _model = new ImageTotal();
                var sentenciaSQLDatos = $@"SELECT  CORE, id, Code,Ruta,Rutaexiste,TipoNegocio,NombreLocal,Dirección,Fecha,GeoLa,GeoLo,[key] FROM dbo.vw_fotos_total_encuestas_david WHERE id='{uri}'AND [key]='{key}'";
                var resultProc = tran.Database.SqlQuery<ImageTotal>(sentenciaSQLDatos).ToList();    
                _model = resultProc.FirstOrDefault();
                _model.ImageData = sentenciaSQL;
                return _model;
            }

        }

        public IList<ImageData> MigracionImage(IList<ConfigParametersImagesAzure> dataMigration, string key, string uri)
        {
            using (var tran = new MardisEngine_TestEntities())
            {
                List<ImageTable> imageTable = new List<ImageTable>();
                foreach (ConfigParametersImagesAzure data in dataMigration)
                {
                    string prefijoCore = data.namecore.Replace("CORE", "");
                    var sqlImageTable = $@"select t.TABLE_SCHEMA as [schema],t.TABLE_NAME as [name] ,'{data.namecore}' as core
                                            from sysobjects so inner join INFORMATION_SCHEMA.TABLES t on(so.name = t.TABLE_NAME)
                                            where so.type = 'U' and name LIKE '{prefijoCore}%' and name LIKE '%_BLB' and t.TABLE_CATALOG = 'MardisEngine_Test';";
                    var lects = tran.Database.SqlQuery<ImageTable>(sqlImageTable).ToList();
                    imageTable.AddRange(lects);

                }
                foreach (ImageTable imageT in imageTable)
                {
                    var sqlUri = $@"select i._URI as uri 
                                            from {imageT.schema}.{imageT.name} i inner join  {imageT.schema}.{imageT.core} c on (i._TOP_LEVEL_AURI=c._URI) 
                                            where c._URI='{uri}'";
                    var uries = tran.Database.SqlQuery<string>(sqlUri).ToList();
                    MemoryStream imageStream = null;


                    foreach (string uriSt in uries)
                    {
                        var sqlCore = $@"select c._URI as id_core,i._URI as id_image,c.RUTA_COD as ruta_cod,i._URI+'.jpg' as [filename],i.VALUE as [value] 
                            from {imageT.schema}.{imageT.name}  i inner join {imageT.schema}.{imageT.core} c on (i._TOP_LEVEL_AURI=c._URI)  where i._URI = '{uriSt}'";
                        var images = tran.Database.SqlQuery<Core>(sqlCore).ToList();
                        foreach (Core image in images)
                        {
                            imageStream = new MemoryStream(image.value.ToArray());
                            AzureStorageUtil.UploadFromStream(imageStream, dataMigration.First().container, image.id_image + ".jpg").Wait();
                            string url = AzureStorageUtil.GetUriFromBlob(dataMigration.First().container, image.id_image + ".jpg");

                            int existe = (from bip in tran.backup_images_project
                                          where bip.id_core == image.id_core && bip.id_image == image.id_image
                                          select bip).Count();

                            if (existe == 0)
                            {
                                if (imageStream != null)
                                {
                                    backup_images_project ima = new backup_images_project()
                                    {
                                        id_core = image.id_core,
                                        id_image = image.id_image,
                                        container = dataMigration.First().container,
                                        project = dataMigration.First().project,
                                        ruta_cod = image.ruta_cod,
                                        filename = image.id_image + ".jpg",
                                        link_azure = url,
                                        //link_visualizador = "http://localhost:19241/viewimages/load?container=" + container
                                        //    + "&id_core=" + image.id_core,
                                        name = imageT.name.Replace(imageT.core, "").Replace("_BLB", "").Replace("IMG_", "").Replace("_IMG", ""),
                                        origen = imageT.name
                                    };
                                    tran.backup_images_project.Add(ima);
                                    tran.SaveChanges();
                                }
                            }
                        }
                    }
                }
                return null;
            }
        }

        public ImageTotal MigrationNull(string id, string key)
        {
            using (var tran = new MardisEngine_TestEntities())
            {

                List<ImageData> DataM = new List<ImageData>();


                List<ImageTable> imageTable = new List<ImageTable>();


                var sentenciaSQL = $@"SELECT  CORE, id, Code,Ruta,Rutaexiste,TipoNegocio,NombreLocal,Dirección,Fecha,GeoLa,GeoLo,[key] FROM dbo.vw_fotos_total_encuestas_david WHERE id='{id}'AND [key]='{key}'";
                var resultProc = tran.Database.SqlQuery<ImageTotal>(sentenciaSQL).ToList();


                foreach (ImageTotal data in resultProc)
                {
                    string prefijoCore = data.CORE.Replace("CORE", "");
                    var sqlImageTable = $@"select t.TABLE_SCHEMA as [schema],t.TABLE_NAME as [name] ,'{data.CORE}' as core
                                            from sysobjects so inner join INFORMATION_SCHEMA.TABLES t on(so.name = t.TABLE_NAME)
                                            where so.type = 'U' and name LIKE '{prefijoCore}%' and name LIKE '%_BLB' and t.TABLE_CATALOG = 'MardisEngine_Test';";
                    var lects = tran.Database.SqlQuery<ImageTable>(sqlImageTable).ToList();
                    imageTable.AddRange(lects);

                }
                foreach (ImageTable imageT in imageTable)
                {
                    var sqlUri = $@"select i._URI as uri 
                                            from {imageT.schema}.{imageT.name} i inner join  {imageT.schema}.{imageT.core} c on (i._TOP_LEVEL_AURI=c._URI) 
                                            where c._URI='{id}'";
                    var uries = tran.Database.SqlQuery<string>(sqlUri).ToList();


                    foreach (string uriSt in uries)
                    {
                        var sqlCore = $@"select i.VALUE as [valueFoto] 
                            from {imageT.schema}.{imageT.name}  i   where i._URI = '{uriSt}'";
                        var images = tran.Database.SqlQuery<ImageData>(sqlCore).ToList();

                        DataM.AddRange(images);
                        DataM.ToList().ForEach(f => f.name = imageT.name.Replace(imageT.core, "").Replace("_BLB", "").Replace("IMG_", "").Replace("_IMG", ""));
                    }

                }





                DataM.ToList().ForEach(f => f.link = String.Format("data:image/gif;base64,{0}", Convert.ToBase64String((byte[])f.valueFoto)));

                ImageTotal ImageTotal = new ImageTotal();
                ImageTotal = resultProc.FirstOrDefault();
                ImageTotal.ImageData = DataM;

                return ImageTotal;
            }
        }

        #endregion
        #region ContextImageData

        public IList<ContextImageData> GetImageContext(string uriImg)
        {
            using (var tran = new MardisEngine_TestEntities())
            {
                var sentenciaSQL = $@"SELECT  DISTINCT CORE ,id ,Code ,Ruta ,[Ruta existe] ,[Tipo Negocio] ,[Nombre Local]
                                ,Dirección ,Fecha ,GeoLa ,GeoLo FROM dbo.vw_fotos_total_encuestas
                                WHERE id ='{uriImg}'";
                var resultProc = tran.Database.SqlQuery<ContextImageData>(sentenciaSQL).ToList();

                return resultProc;
            }
        }

        #endregion
    }
}