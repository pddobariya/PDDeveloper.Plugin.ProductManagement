using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using GBS.Plugin.ProductManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Extensions;

namespace GBS.Plugin.ProductManagement.Data
{
    /// <summary>
    /// Represents plugin object context
    /// </summary>
    public class ProductManagementObjectContext : DbContext, IDbContext
    {
        #region Const

        private const string procedureStringPath = "~/Plugins/GBS.Plugin.ProductManagement/SqlProcedure/GBS_GetProductIdBySegmentId.sql";
        
        #endregion

        #region Ctor

        public ProductManagementObjectContext(DbContextOptions<ProductManagementObjectContext> options) : base(options)
        {
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Further configuration the model
        /// </summary>
        /// <param name="modelBuilder">Model muilder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProductSegmentMap());
            modelBuilder.ApplyConfiguration(new ProductFilterOptionsMap());
            modelBuilder.ApplyConfiguration(new Product_Include_ExcludeMap());
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Modify the input SQL query by adding passed parameters
        /// </summary>
        /// <param name="sql">The raw SQL query</param>
        /// <param name="parameters">The values to be assigned to parameters</param>
        /// <returns>Modified raw SQL query</returns>
        protected virtual string CreateSqlWithParameters(string sql, params object[] parameters)
        {
            //add parameters to sql
            for (var i = 0; i <= (parameters?.Length ?? 0) - 1; i++)
            {
                if (!(parameters[i] is DbParameter parameter))
                    continue;

                sql = $"{sql}{(i > 0 ? "," : string.Empty)} @{parameter.ParameterName}";

                //whether parameter is output
                if (parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Output)
                    sql = $"{sql} output";
            }

            return sql;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a DbSet that can be used to query and save instances of entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>A set for the given entity type</returns>
        public virtual new DbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }

        /// <summary>
        /// Generate a script to create all tables for the current model
        /// </summary>
        /// <returns>A SQL script</returns>
        public virtual string GenerateCreateScript()
        {
            return this.Database.GenerateCreateScript();
        }

        /// <summary>
        /// Creates a LINQ query for the query type based on a raw SQL query
        /// </summary>
        /// <typeparam name="TQuery">Query type</typeparam>
        /// <param name="sql">The raw SQL query</param>
        /// <returns>An IQueryable representing the raw SQL query</returns>
        public virtual IQueryable<TQuery> QueryFromSql<TQuery>(string sql) where TQuery : class
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a LINQ query for the entity based on a raw SQL query
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="sql">The raw SQL query</param>
        /// <param name="parameters">The values to be assigned to parameters</param>
        /// <returns>An IQueryable representing the raw SQL query</returns>
        public virtual IQueryable<TEntity> EntityFromSql<TEntity>(string sql, params object[] parameters) where TEntity : BaseEntity
        {
            return this.Set<TEntity>().FromSql(CreateSqlWithParameters(sql, parameters), parameters);
        }
        
        /// <summary>
        /// Executes the given SQL against the database
        /// </summary>
        /// <param name="sql">The SQL to execute</param>
        /// <param name="doNotEnsureTransaction">true - the transaction creation is not ensured; false - the transaction creation is ensured.</param>
        /// <param name="timeout">The timeout to use for command. Note that the command timeout is distinct from the connection timeout, which is commonly set on the database connection string</param>
        /// <param name="parameters">Parameters to use with the SQL</param>
        /// <returns>The number of rows affected</returns>
        public virtual int ExecuteSqlCommand(RawSqlString sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters)
        {
            using (var transaction = this.Database.BeginTransaction())
            {
                var result = this.Database.ExecuteSqlCommand(sql, parameters);
                transaction.Commit();

                return result;
            }
        }

        /// <summary>
        /// Detach an entity from the context
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        public virtual void Detach<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Install object context
        /// </summary>
        public void Install()
        {
            try
            {
                //create the table
                string dbScript = GenerateCreateScript();
                string dbInstallationScript = string.Empty;
                string goString = Environment.NewLine;
                goString += "GO";
                goString += Environment.NewLine;
                string[] scriptBlocks = dbScript.Split(';');
                for (int i = 0; i < scriptBlocks.Length; i++)
                {
                    string scriptBlock = scriptBlocks[i].Replace(goString, "").Trim();
                    if (!string.IsNullOrEmpty(scriptBlock))
                    {
                        int firstBracketIndex = scriptBlock.IndexOf('[');
                        string tableName = scriptBlock.Substring(firstBracketIndex, (scriptBlock.IndexOf('(') - firstBracketIndex)).Trim();
                        tableName = tableName.Replace("[", string.Empty).Replace("]", string.Empty);
                        if (!string.IsNullOrEmpty(dbInstallationScript))
                            dbInstallationScript += Environment.NewLine;
                        dbInstallationScript += "IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='" + tableName + "')";
                        dbInstallationScript += Environment.NewLine + "BEGIN" + Environment.NewLine;
                        dbInstallationScript += scriptBlock + ";";
                        dbInstallationScript += Environment.NewLine + "END";
                        dbInstallationScript += Environment.NewLine;
                    }
                }
                dbInstallationScript += "GO";
                this.ExecuteSqlScript(dbInstallationScript);

                dbInstallationScript = string.Empty;
                dbInstallationScript += "IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('dbo.GBS_GetProductIdBySegmentId'))";
                dbInstallationScript += Environment.NewLine;
                dbInstallationScript += "BEGIN";
                dbInstallationScript += Environment.NewLine;
                dbInstallationScript += "DROP PROCEDURE [dbo].[GBS_GetProductIdBySegmentId]";
                dbInstallationScript += Environment.NewLine;
                dbInstallationScript += "END";
                this.ExecuteSqlScript(dbInstallationScript);

                var _fileProvider = EngineContext.Current.Resolve<INopFileProvider>();
                dbInstallationScript = File.ReadAllText(_fileProvider.MapPath(procedureStringPath));
                this.ExecuteSqlScript(dbInstallationScript);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Uninstall object context
        /// </summary>
        public void Uninstall()
        {
            //drop the table
            this.DropPluginTable(nameof(ProductSegment));
            this.DropPluginTable(nameof(ProductFilterOptions));
            this.DropPluginTable(nameof(Product_Include_ExcludeMap));

            //Drop Prodecure
            string dbInstallationScript = string.Empty;
            dbInstallationScript += "IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('dbo.GBS_GetProductIdBySegmentId'))";
            dbInstallationScript += Environment.NewLine;
            dbInstallationScript += "BEGIN";
            dbInstallationScript += Environment.NewLine;
            dbInstallationScript += "DROP PROCEDURE [dbo].[GBS_GetProductIdBySegmentId]";
            dbInstallationScript += Environment.NewLine;
            dbInstallationScript += "END";
            Database.ExecuteSqlCommand(dbInstallationScript);
        }

        #endregion
    }
}