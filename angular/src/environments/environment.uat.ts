import { Environment } from '@abp/ng.core';

const baseUrl = 'https://ione-uat.vnexco.com';

const oAuthConfig = {
    issuer: 'https://ione-api-uat.vnexco.com/',
    redirectUri: baseUrl,
    clientId: 'iOne_App',
    responseType: 'code', // Use Authorization Code Flow (same as ABP project)
    scope: 'offline_access iOne',
    requireHttps: true,
};

export const environment = {
    production: false,
    application: {
        baseUrl,
        name: 'HỆ THỐNG BẢO HIỂM PHI NHÂN THỌ (UAT)',
    },
    oAuthConfig,
    apis: {
        default: {
            url: 'https://ione-api-uat.vnexco.com',
            rootNamespace: 'iOne',
        },
        AbpAccountPublic: {
            url: oAuthConfig.issuer,
            rootNamespace: 'AbpAccountPublic',
        },
    },
} as Environment;
