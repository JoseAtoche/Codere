He desarrollado esta aplicación de una forma no muy compleja, permitiendo lo máximo posible poder expandir sin demasiados cambios,

He creado un objeto dto que sería el que insertamos en la BBDD, y luego otro objeto que es el que importo de la API, esto lo he hecho así por si, imaginariamente, el modelo de la API que consumimos cambiase de un día para otro que la BBDD no se viese modificada ni tuviese que hacerse una modificación forzosa a contrarreloj, simplemente deberíamos hacer algún cambio al modelo y no mapear esos nuevos datos de ser necesario. Cuando estuviese lista y preparada la BBDD sería cuando se modificaría el DTO.

En este caso la Aplicación la he desarrollado usando SQLite, la razon es sencilla, mayor facilidad(en este caso) para enviaros la BBDD, y además eran pocos datos.
Si hubiese desarrollado esta aplicación de forma más profesional, sin lugar a dudas lo hubiese desarrollado con SQLServer, creo que sería lo más óptimo, así podría ponerse en servidores externos, etc, pero repito, para este caso lo más sencillo me parecía esto, sobre todo para la hora de entregar el trabajo.

Lo que más he dudado ha sido a la hora de implementar la “api key” ya que al principio pensé incluso en desarrollar un sistema de Tokens(JwtBearer), para que el usuario tuviese que llamar a una api para obtener el token y luego poder realizar las llamadas pertinentes a la API.
Pero en este caso, ya que así lo describía el requisito me decanté por usar efectivamente un API Key, creo que en el caso de querer una aplicación más segura debería implementarse un sistema de Tokens

He pensado que para este caso en el middleware de seguridad podíamos poner las rutas que son públicas en una lista para así tenerlo más localizado.

En el caso de haberlo hecho con Bearer hubiese sido tan sencillo como hacer un método con la cabecera “ [AllowAnonymous] ” para que permitiese sin problemas al usuario llamar a dichos métodos.

Me he tomado también la libertad de crear un pequeño método que me permite, desde una llamada API, lanzar una Query, creo que es algo interesante en este caso.

Según lo que he entendido en el Requerimiento 2, parece que el método del requerimiento 1 debe lanzarse solo cuando se invoca al método, por ello así lo he hecho.

También he comprobado a la hora de importar que los objetos que vienen de la API no estén en la BBDD, para no duplicar información y también compruebo si están modificados (por si los datos que envía la API cambia, que llegue a importar y modificar los cambios).

A las clases internas del objeto Show le he creado unos IDs autoincrementales, para así agilizar el proceso de búsqueda si llegase a haber demasiados datos.



--------------------------------


Como ejecutar la aplicacion


----------------------------

Simplemente descargue el código, y con Visual Studio abralo y ejecútelo, La versión de VS que he usado en mi caso es VS 2022 (por si hubiese algun problema)

En el caso de que la BBDD Sqlite no exista no habría problema, se crea de forma automática cuando la aplicación se ejecuta.

Para poder lanzar comandos es tan simple como abrir un programa como Postman y llamar a la siguiente ruta para importar los datos:

En Headers es importante especificar lo siguiente:
ApiKey - DevelopKey
Content-Type - application/json

https://localhost:44360/api/Shows/ShowsMainInformationAndImport

https://localhost:44360/api/Shows/show/{id} (siendo el número del Show a visualizar)
https://localhost:44360/api/Shows/showAllData (método público, no necesita el Header ApiKey)
https://localhost:44360/api/Shows/GetDataByQuery
Este método necesita en el Body algo como esto:
"select  show.* from 'Show' where id= 1"










