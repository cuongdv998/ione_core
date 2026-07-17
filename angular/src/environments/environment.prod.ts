import { Environment } from '@abp/ng.core';

const baseUrl = 'https://ione.vnexco.com';

const oAuthConfig = {
    issuer: 'https://api.ibds.com.vn/',
    redirectUri: baseUrl,
    clientId: 'iOne_App',
    responseType: 'code', // Use Authorization Code Flow (same as ABP project)
    scope: 'offline_access iOne',
    requireHttps: true,
};

export const environment = {
    production: true,
    application: {
        baseUrl,
        name: 'HỆ THỐNG BẢO HIỂM PHI NHÂN THỌ',
    },
    oAuthConfig,
    apis: {
        default: {
            url: 'https://api.ibds.com.vn',
            rootNamespace: 'iOne',
        },
        AbpAccountPublic: {
            url: oAuthConfig.issuer,
            rootNamespace: 'AbpAccountPublic',
        },
    },
} as Environment;
