abp install-libs

cd src/common/infra/iOne.DbMigrator && dotnet run && cd -


cd src/web/iOne.HttpApi.Host && dotnet dev-certs https -v -ep openiddict.pfx -p config.auth_server_default_pass_phrase 



exit 0