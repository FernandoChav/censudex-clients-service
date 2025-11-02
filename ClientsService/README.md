#  Censudex - Servicio de Clientes (Taller 2)

Este proyecto es el **Servicio de Clientes**. Su único trabajo es crear, encontrar y guardar usuarios en una base de datos.

Sigue estos 3 pasos para hacerlo funcionar en tu computadora.

---

## Paso 1: Configura la Contraseña

Vamos a usar **una sola contraseña** para todo. Así es más fácil.

1.  Abre el archivo que está en `ClientsService/appsettings.json`.
2.  Busca esta línea:
    `"DefaultConnection": "Host=localhost;Port=5432;...;Password=tu_password"`
3.  Borra `"tu_password"` y escribe una contraseña fácil de recordar.

    **Recomendación:** Usemos `admin123` para esta guía.
    Debería quedarte así:
    `"DefaultConnection": "Host=localhost;...;Password=admin123"`
4.  **Guarda el archivo.**

---

## Paso 2: Inicia la Base de Datos (con Docker)

Nuestra aplicación necesita una base de datos para guardar cosas. Usaremos Docker para esto.

1.  Abre **Docker Desktop** y asegúrate de que esté corriendo (el icono de la ballena debe estar verde).
2.  Abre tu **Terminal** (CMD, PowerShell, o Terminal en Mac).
3.  Copia y pega el siguiente comando. Este comando usa la contraseña `admin123` que elegimos en el Paso 1.

    ```bash
    docker run -d --name censudex-postgres -p 5432:5432 -e POSTGRES_PASSWORD=admin123 -e POSTGRES_USER=postgres -e POSTGRES_DB=censudex_clients postgres:latest
    ```
4.  Presiona **Enter**.

* *(Nota: La primera vez, esto tardará un minuto porque tiene que "descargar" Postgres. ¡Es normal!)*
* *(Nota 2: Si ya lo habías creado y solo está detenido, el comando es: `docker start censudex-postgres`)*

¡Listo! Tu base de datos ya está corriendo.

---

##  Paso 3: ¡Inicia la Aplicación!

Ahora que la base de datos está lista, podemos iniciar la aplicación.

1.  En la **misma terminal**, asegúrate de estar en la carpeta raíz del proyecto.
2.  Entra en la carpeta del servicio:
    ```bash
    cd ClientsService
    ```
3.  **(Opcional pero recomendado)** Descarga todos los paquetes del proyecto:
    ```bash
    dotnet restore
    ```
4.  Escribe el siguiente comando y presiona **Enter**:
    ```bash
    dotnet run
    ```
5.  Espera unos segundos. El código se conectará a la base de datos y creará las tablas.
6.  **Sabrás que funcionó** cuando veas este mensaje en tu terminal:

    `info: Microsoft.Hosting.Lifetime[14]`
    `      Now listening on: https://localhost:7001`

**¡FELICIDADES! Tu servicio está vivo y corriendo.**

---

##  Guía de Pruebas (Postman)

Aquí tienes los ejemplos de JSON para probar **todos los métodos** del servicio.

### Configuración Rápida de Postman

1.  Abre Postman y crea una **nueva solicitud gRPC**.
2.  En la URL, escribe: `localhost:7001`
3.  Ve a `Settings` ⚙️ > `General` y **apaga** (`OFF`) la opción `SSL certificate verification`.
4.  Haz clic en **"Import a .proto"** e importa el archivo `ClientsService/Protos/clients.proto`.
5.  En el menú de la izquierda, selecciona el método que quieres probar.

---

### 1. Método: `CreateClient`
* **Para qué es:** Crear un nuevo usuario.
* **Mensaje (JSON):**
    ```json
    {
      "first_name": "TestUser",
      "last_name": "Dos",
      "email": "test2@censudex.cl",
      "username": "testuser2",
      "birth_date": "1999-05-10",
      "address": "Calle Falsa 123",
      "phone_number": "+56944445555",
      "password": "PasswordValida123!"
    }
    ```
* **Resultado esperado:** Un `ClientResponse` con los datos del usuario y un `id` nuevo.

### 2. Método: `GetAllClients`
* **Para qué es:** Ver la lista de todos los clientes **activos**.
* **Mensaje (JSON):** No necesita mensaje. Solo presiona **`Invoke`** con el mensaje vacío (`{}`).
* **Resultado esperado:** Una lista (`"clients": [...]`) con el `admin` y los usuarios que hayas creado.

### 3. Método: `GetClientById`
* **Para qué es:** Buscar un solo cliente por su ID.
* **Paso Previo:** Ejecuta `GetAllClients` primero y **copia el `id`** de un usuario.
* **Mensaje (JSON):**
    ```json
    {
      "id": "AQUI_PEGA_EL_ID_QUE_COPIASTE"
    }
    ```
* **Resultado esperado:** Un `ClientResponse` con los datos de ese usuario.

### 4. Método: `UpdateClient`
* **Para qué es:** Actualizar los datos de un cliente.
* **Paso Previo:** Ejecuta `GetAllClients` y **copia el `id`** de un usuario.
* **Mensaje (JSON):**
    ```json
    {
      "id": "AQUI_PEGA_EL_ID_QUE_COPIASTE",
      "first_name": "NombreActualizado",
      "last_name": "ApellidoActualizado",
      "address": "Nueva Direccion 456",
      "phone_number": "+56911112222",
      "birth_date": "1999-05-10"
    }
    ```
* **Resultado esperado:** Un `ClientResponse` con los datos actualizados.

### 5. Método: `DeleteClient` (Soft Delete)
* **Para qué es:** Marcar a un usuario como inactivo.
* **Paso Previo:** Ejecuta `GetAllClients` y **copia el `id`** de un usuario.
* **Mensaje (JSON):**
    ```json
    {
      "id": "AQUI_PEGA_EL_ID_QUE_COPIASTE"
    }
    ```
* **Resultado esperado:** Un mensaje de éxito (`"statusMessage": "Client deactivated successfully"`).
* **Verificación:** Llama a `GetAllClients` de nuevo. El usuario borrado **ya no debe aparecer**.

### 6. Método: `GetClientForAuth`
* **Para qué es:** Método especial para el login. Busca por email o username.
* **Mensaje (JSON):**
    ```json
    {
      "email_or_username": "admin@censudex.cl"
    }
    ```
* **Resultado esperado:** Un `ClientAuthResponse` que incluye el `password_hash` y el `role`.