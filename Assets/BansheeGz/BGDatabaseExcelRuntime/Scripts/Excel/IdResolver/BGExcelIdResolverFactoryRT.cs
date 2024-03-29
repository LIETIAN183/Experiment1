/*
<copyright file="BGExcelIdResolverFactory.cs" company="BansheeGz">
    Copyright (c) 2018-2021 All Rights Reserved
</copyright>
*/

using System;

namespace BansheeGz.BGDatabase
{
    public class BGExcelIdResolverFactoryRT
    {
        private readonly BGRepo mainRepo;
        private readonly BGSyncIdConfig idConfig;

        public BGExcelIdResolverFactoryRT(BGRepo mainRepo, BGSyncIdConfig idConfig)
        {
            this.mainRepo = mainRepo;
            this.idConfig = idConfig;
        }

        public BGExcelIdResolverART Create(BGId metaId, BGLogger logger)
        {
            if (idConfig == null) return new BGExcelIdResolverIdRT(logger);
            var metaConfig = idConfig.GetMetaConfig(metaId);
            if (metaConfig == null) return new BGExcelIdResolverIdRT(logger);
            switch (metaConfig.configType)
            {
                case BGSyncIdConfig.IdConfigEnum.IdColumn:
                    return new BGExcelIdResolverIdRT(logger);
                case BGSyncIdConfig.IdConfigEnum.NoId:
                    return new BGExcelIdResolverNoIdRT(logger);
                case BGSyncIdConfig.IdConfigEnum.Index:
                    return new BGExcelIdResolverIndexRT(logger, mainRepo == null ? null : mainRepo.GetMeta(metaId));
                case BGSyncIdConfig.IdConfigEnum.Field:
                    if (mainRepo == null) return new BGExcelIdResolverIdRT(logger);
                    var mainMeta = mainRepo.GetMeta(metaId);
                    if (mainMeta == null) return new BGExcelIdResolverIdRT(logger);
                    var field = mainMeta.GetField(metaConfig.FieldId, false);
                    if (field == null || !BGSyncIdConfig.IsSupported(field)) return new BGExcelIdResolverIdRT(logger);
                    if (field is BGFieldString) return new BGExcelIdResolverFieldStringRT(logger, (BGFieldString) field);
                    if (field is BGFieldInt) return new BGExcelIdResolverFieldIntRT(logger, (BGFieldInt) field);
                    throw new BGException("Can not create id resolver, field is " + field.GetType());
                default:
                    throw new ArgumentOutOfRangeException("metaConfig.configType", "Unsupported config type=" + metaConfig.configType);
            }
        }
    }
}