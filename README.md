# APIBaseLib

APIBaseLib é uma biblioteca base em .NET para a construção de APIs RESTful. Este projeto fornece uma estrutura genérica para criar serviços e controladores com suporte a paginação, operações CRUD e HATEOAS.

## Intalação

Para instalar a APIBaseLib em seu projeto .NET, siga estas etapas:

1. Abra o terminal na pasta raiz do seu projeto.

2. Execute o seguinte comando:

  ```bash
  dotnet add package APIBaseLib --version 1.0.5
  ```

  Este comando adicionará a versão 1.0.5 da APIBaseLib ao seu projeto.

3. Verifique se a referência foi adicionada corretamente no arquivo .csproj do seu projeto.

## Uso Inicial

Após a instalação, você pode começar a usar a APIBaseLib em seu projeto:

1. Adicione a seguinte diretiva using no topo dos arquivos onde você deseja usar a biblioteca:

```csharp
using APIBaseLib;

## Funcionalidades

- CRUD genérico para qualquer entidade que herde de `BaseEntity`.
- Paginação para listagens.
- HATEOAS para navegação entre rotas da API.
- Tratamento de erros padronizado.
- Suporte a rotas RESTful completas, incluindo GET, POST, PUT, PATCH e DELETE.

## Requisitos

- .NET 8.0 ou superior.
- Docker.
- PostgreSQL.

## Configuração do Banco de Dados

Este projeto utiliza o PostgreSQL como banco de dados, configurado através do Docker. Siga as instruções no repositório [database-pgsql-docker](https://github.com/KleberSouza/database-pgsql-docker) para configurar o ambiente do banco de dados.

Após a configuração do banco de dados com Docker, atualize a string de conexão no `appsettings.json` do seu projeto para apontar para o banco de dados PostgreSQL em execução:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=YourDatabase;Username=YourUsername;Password=YourPassword"
  }
}
```

## Criação de um Novo Model

1. Crie um novo model que herde de **BaseEntity**:

    ```csharp
    using APIBaseLib.Models;

    namespace YourNamespace.Models
    {
        public class Product : BaseEntity
        {
            public string Name { get; set; }
            public decimal Price { get; set; }
        }
    }
    ```

2. Crie um repositório novo model:

    ```csharp
    using APIBaseLib.Repositories;
    using YourNamespace.Models;

    namespace YourNamespace.Repositories
    {
        public interface IProductRepository : IRepository<Product> { }

        public class ProductRepository : Repository<Product, YourDbContext>, IProductRepository
        {
            public ProductRepository(YourDbContext context) : base(context) { }
        }
    }
    ```

## Exemplo de Serviço

Crie um serviço para o seu model que herde de Service<TEntity, TContext>:

```csharp
using APIBaseLib.Services;
using YourNamespace.Models;

namespace YourNamespace.Services
{
    public interface IProductService : IService<Product> { }

    public class ProductService : Service<Product, YourDbContext>, IProductService
    {
        public ProductService(IProductRepository repository) : base(repository) { }
    }
}

```

## Exemplo de Controller

Crie um controller que herde de BaseController<TEntity, TService>:

```csharp
using APIBaseLib.Controllers;
using YourNamespace.Models;
using YourNamespace.Services;
using Microsoft.AspNetCore.Mvc;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    public class ProductsController : BaseController<Product, IProductService>
    {
        public ProductsController(IProductService service) : base(service) { }
    }
}

```

## Registro do serviço e do repositório

Registre o repositório e o serviço no Startup.cs ou Program.cs:

```csharp
services.AddScoped<IProductRepository, ProductRepository>();
services.AddScoped<IProductService, ProductService>();
```

## Exemplo de Utilização

Depois de configurar o projeto, você pode começar a utilizar a API executando o servidor com:

```csharp
dotnet run
```

## Rotas Disponíveis

Aqui estão as rotas padrão disponíveis para um model, como **Product**:

- **GET /api/products**: Retorna uma lista paginada de produtos.
- **GET /api/products/{id}**: Retorna um produto específico pelo ID.
- **POST /api/products**: Cria um novo produto.
- **PUT /api/products/{id}**: Atualiza um produto existente pelo ID.
- **PATCH /api/products/{id}**: Atualiza campos específicos de um produto.
- **DELETE /api/products/{id}**: Exclui um produto pelo ID.

## Exemplo de Execução

1. Criar um Produto
   
```json
curl -X POST "https://localhost:5001/api/products" \
-H "Content-Type: application/json" \
-d '{
  "name": "Product 1",
  "price": 10.99
}'

```

Resposta:

```json
{
  "data": {
    "id": 1,
    "name": "Product 1",
    "price": 10.99
  },
  "links": {
    "self": "https://localhost:5001/api/products/1",
    "update": "https://localhost:5001/api/products/1",
    "delete": "https://localhost:5001/api/products/1"
  }
}

```

2. Obter Todos os Produtos
   
```json
curl -X GET "https://localhost:5001/api/products"
```

Resposta:

```json
{
  "data": [
    {
      "id": 1,
      "name": "Product 1",
      "price": 10.99
    },
    {
      "id": 2,
      "name": "Product 2",
      "price": 20.99
    }
  ],
  "links": {
    "self": "https://localhost:5001/api/products"
  }
}

```

3. Obter um Produto pelo ID
   
```json
curl -X GET "https://localhost:5001/api/products/1"
```

Resposta:

```json
{
  "data": {
    "id": 1,
    "name": "Product 1",
    "price": 10.99
  },
  "links": {
    "self": "https://localhost:5001/api/products/1",
    "update": "https://localhost:5001/api/products/1",
    "delete": "https://localhost:5001/api/products/1"
  }
}

```

4. Atualizar um Produto

```json
curl -X PUT "https://localhost:5001/api/products/1" \
-H "Content-Type: application/json" \
-d '{
  "name": "Updated Product",
  "price": 15.99
}'

```

Resposta:

```json
{
  "data": {
    "id": 1,
    "name": "Updated Product",
    "price": 15.99
  },
  "links": {
    "self": "https://localhost:5001/api/products/1",
    "update": "https://localhost:5001/api/products/1",
    "delete": "https://localhost:5001/api/products/1"
  }
}
```
5. Atualizar Campos Específicos de um Produto (PATCH)

```json
curl -X PATCH "https://localhost:5001/api/products/1" \
-H "Content-Type: application/json" \
-d '{
  "price": 18.99
}'
```

Resposta:

```json
curl -X PATCH "https://localhost:5001/api/products/1" \
-H "Content-Type: application/json" \
-d '{
  "price": 18.99
}'
```

6. Excluir um Produto

```json
curl -X DELETE "https://localhost:5001/api/products/1"
```

Resposta:

```json
{
  "message": "Product with ID 1 has been deleted."
}
```

## Contribuição

Sinta-se à vontade para contribuir com este projeto enviando pull requests ou reportando issues.

## Licença

Este projeto está licenciado sob os termos da licença MIT.
