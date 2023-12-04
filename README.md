## Español
## Descripción

Esta aplicación ha sido desarrollada con el objetivo de mantener la simplicidad y la flexibilidad para futuras expansiones sin realizar cambios significativos.

## Evolución de la aplicación

Pueden leer la descripción de los Commits, ya que en cada uno voy indicando los pequeños evolutivos que he realizado y que se ha modificado en cada caso.

## Ejecución de la Aplicación

Antes de abrir la aplicación recomiendo tener Visual Studio 2022 y .NET 8.0 instalados, ya que son las versiones concretas que he usado para desarrollar la aplicación.

1. Descargue el código y ábralo en Visual Studio (Versión utilizada: VS 2022).
2. Ejecute la aplicación. (está desarrollada en .Net 8.0)
3. En este momento (si no está creada) verá crearse el archivo de BBDD Sqlite.
4. Abra Postman y use los comandos de API escritos más abajo en el documento.

### Estructura del Proyecto

Se han creado dos objetos clave:

1. **DTO (Data Transfer Object):** Representa los datos que se insertan en la base de datos.
2. **Objeto de Importación desde la API:** Utilizado para importar datos desde la API.

Esta estructura permite una separación clara entre la API y la base de datos, facilitando adaptaciones futuras si el modelo de la API cambia.

### Capa de Servicio

Se ha introducido una capa de servicio (`ShowsService`) que actúa como intermediario entre los controladores y la capa de acceso a datos (`ShowsRepository`). Esta capa proporciona una abstracción adicional para realizar operaciones específicas del negocio y lógica de importación de datos.
Tambien servirá para poder ser reaprovechada en otro tipo de proyecto, como uno Web o un proyecto HangFire, por ejemplo.

### Base de Datos

La aplicación utiliza SQLite debido a su simplicidad y facilidad para compartir la base de datos con ustedes. En un entorno más profesional, se consideraría el uso de SQL Server para mayor escalabilidad.

### Seguridad

En lugar de implementar inicialmente un sistema de tokens (JwtBearer), opté por una solución más simple utilizando API Key. Para mejorar la seguridad, se recomendaría un sistema de tokens en un entorno más robusto. Es importante destacar que el API Key es expresamente lo que se pide en el Requisito 2.

### Middleware de Seguridad

Las rutas públicas se gestionan en el middleware de seguridad para una fácil identificación y control. Esto se configura en `Program.cs` mediante la lista `publicPaths`.

### Importación de Datos

Se ha implementado un método que permite lanzar una consulta desde una llamada API. Además, al importar datos, se verifica la existencia y la modificación de objetos para evitar duplicaciones y mantener la integridad. No se realiza una importación automática al iniciar la aplicación porque, según lo que entendí en los Requerimientos, se quería solo cuando se llamara al método API concreto.
He tenido tambien en cuenta los datos duplicados, tanto de Networks como de Countries, para no insertar en BBDD datos duplicados, si llega a cambiar los datos, si se vuelven a importar se modificarían/añadirían de nuevo.

### IDs Autoincrementales

Se han añadido IDs autoincrementales a las clases internas del objeto `Show` para agilizar la búsqueda en caso de una gran cantidad de datos.


### BBDD SQLite

La base de datos SQLite se crea automáticamente al ejecutar la aplicación. (No se importan los datos en este momento)

## Comandos de API

| Descripción                       | URL                                              |
| ----------------------------------| --------------------------------------------------|
| Importar Datos Principales         | `https://localhost:44360/api/Shows/ShowsMainInformationAndImport` |
| Visualizar Detalles de un Show por ID      | `https://localhost:44360/api/Shows/show/{id}`                     |
| Mostrar Todos los Datos (Método Público) | `https://localhost:44360/api/Shows/showAllData`              |
| Obtener Datos por Consulta         | `https://localhost:44360/api/Shows/GetDataByQuery`               |

### Encabezados Importantes

Añada estos encabezados en todos los métodos, el APIKey solo es prescindible en el método señalado como "Método público"

- `ApiKey - DevelopKey`
- `Content-Type - application/json`

### Body necesario para Obtener Datos por Consulta:

```json
"select show.* from 'Show' where id=1"
```



## English


## Description

This application has been developed with the goal of maintaining simplicity and flexibility for future expansion without making significant changes.

## Evolution of the application

You can read the description of the Commits, since in each one I am indicating the small evolutions that I have made and what has been modified in each case.

## Application Execution

Before opening the application I recommend to have Visual Studio 2022 and .NET 8.0 installed, since they are the concrete versions that I have used to develop the application.

1. Download the code and open it in Visual Studio (Version used: VS 2022).
2. Run the application (it is developed in .Net 8.0).
3. At this point (if it is not already created) you will see the Sqlite DB file being created.
4. Open Postman and use the API commands written below in the document.

### Project Structure

Two key objects have been created:

1. **DTO (Data Transfer Object):** Represents the data that is inserted into the database.
2. **Import Object from API:** Used to import data from the API.

This structure allows a clear separation between the API and the database, facilitating future adaptations if the API model changes.

### Service Layer

A service layer (`ShowsService`) has been introduced to act as an intermediary between the controllers and the data access layer (`ShowsRepository`). This layer provides an additional abstraction to perform business specific operations and data import logic.
It will also serve to be reused in another type of project, such as a Web or HangFire project, for example.

### Database

The application uses SQLite because of its simplicity and ease of sharing the database with you. In a more professional environment, SQL Server would be considered for scalability.

### Security

Instead of initially implementing a token system (JwtBearer), I opted for a simpler solution using API Key. To improve security, a token system in a more robust environment would be recommended. It is important to note that API Key is expressly what is requested in Requirement 2.

### Security Middleware

Public routes are managed in the security middleware for easy identification and control. This is configured in `Program.cs` via the `publicPaths` list.

### Data Import

A method has been implemented that allows to launch a query from an API call. In addition, when importing data, objects are checked for existence and modification to avoid duplication and maintain integrity. An automatic import is not performed when starting the application because, according to what I understood in the Requirements, it was wanted only when the particular API method was called.
I have also taken into account duplicate data, both from Networks and Countries, in order not to insert duplicate data in the DB, if the data changes, if it is imported again it would be modified/added again.

### Auto-incremental IDs

Auto-incremental IDs have been added to the inner classes of the `Show` object to speed up the search in case of a large amount of data.


### SQLite database

The SQLite database is automatically created when the application is run (No data is imported at this time).

## API commands

| Description | URL |
| ----------------------------------| --------------------------------------------------|
| Import Main Data | `https://localhost:44360/api/Shows/ShowsMainInformationAndImport` |
| Display Show Details by ID | `https://localhost:44360/api/Shows/show/{id}` |
| Display All Data (Public Method) | `https://localhost:44360/api/Shows/showAllData` |
| Retrieve Data by Query | `https://localhost:44360/api/Shows/GetDataByQuery` |

### Important Headings

Add these headers in all methods, the APIKey is only dispensable in the method marked as "Public Method".

- `ApiKey - DevelopKey`
- `Content-Type - application/json`

### Body required to obtain data by Query:

```json
"select show.* from 'Show' where id=1"
```
