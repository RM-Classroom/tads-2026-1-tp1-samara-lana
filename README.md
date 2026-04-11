# Projeto Locadora de Veículos

## Como testar a criação do Banco de Dados (Entity Framework Core)

Para avaliar os modelos e o banco de dados da aplicação, siga estes passos:

1. Abra a solução (LocadoraVeiculosApi.slnx ou arquivo .csproj) no Visual Studio.
2. Certifique-se de que a Connection String no ppsettings.json (ou ppsettings.Development.json) esteja configurada corretamente para o seu ambiente local (ex: LocalDB do SQL Server).
3. No Visual Studio, acesse: **Ferramentas > Gerenciador de Pacotes do NuGet > Console do Gerenciador de Pacotes**.
4. No console, execute o comando abaixo para aplicar as *Migrations* e gerar o banco:
   `powershell
   Update-Database
   `
5. Você poderá visualizar as tabelas geradas de acordo com as entidades da pasta Models inspecionando o banco através do *Pesquisador de Objetos do SQL Server*.
