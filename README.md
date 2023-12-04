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

