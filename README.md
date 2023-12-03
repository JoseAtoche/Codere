## Descripción

He desarrollado esta aplicación con el objetivo de mantener la simplicidad y la flexibilidad para futuras expansiones sin realizar cambios significativos.

### Estructura del Proyecto

He creado dos objetos clave:

1. **DTO (Data Transfer Object):** Representa los datos que se insertan en la base de datos.
2. **Objeto de Importación desde la API:** Utilizado para importar datos desde la API.

Esta estructura permite una separación clara entre la API y la base de datos, facilitando adaptaciones futuras si el modelo de la API cambia.

### Base de Datos

La aplicación utiliza SQLite debido a su simplicidad y facilidad para compartir la base de datos. En un entorno más profesional, se consideraría el uso de SQL Server para mayor escalabilidad.

### Seguridad

En lugar de implementar inicialmente un sistema de tokens (JwtBearer), opté por una solución más simple utilizando API Key. Para mejorar la seguridad, se recomendaría un sistema de tokens en un entorno más robusto.
He de añadir que el API Key es expresamente lo que se pide en el Requisito 2

### Middleware de Seguridad

Las rutas públicas se gestionan en el middleware de seguridad para una fácil identificación y control. Dentro de Program.cs y "publicPaths"

### Importación de Datos

Implementé un método que permite lanzar una consulta desde una llamada API. Además, al importar datos, se verifica la existencia y la modificación de objetos para evitar duplicaciones y mantener la integridad.
No he hecho una importación automática al iniciar la aplicación porque según lo que entendí en los Reequerimientos se quería solo cuando se llamase al metodo API concreto.

### IDs Autoincrementales

Se añadieron IDs autoincrementales a las clases internas del objeto `Show` para agilizar la búsqueda en caso de una gran cantidad de datos.

## Ejecución de la Aplicación

1. Descargue el código y ábralo en Visual Studio (Versión utilizada: VS 2022).
2. Ejecute la aplicación.

### BBDD SQLite

La base de datos SQLite se crea automáticamente al ejecutar la aplicación.

## Comandos de API

| Descripción                       | URL                                              |
| ----------------------------------| --------------------------------------------------|
| Importar Datos Principales         | `https://localhost:44360/api/Shows/ShowsMainInformationAndImport` |
| Visualizar Detalles de un Show     | `https://localhost:44360/api/Shows/show/{id}`                     |
| Mostrar Todos los Datos (Método Público) | `https://localhost:44360/api/Shows/showAllData`              |
| Obtener Datos por Consulta         | `https://localhost:44360/api/Shows/GetDataByQuery`               |

### Encabezados Importantes

- `ApiKey - DevelopKey`
- `Content-Type - application/json`

