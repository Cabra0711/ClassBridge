# IUE Desatrasador — MVP

## ⚡ PASOS PARA EJECUTAR (5 minutos)

### 1. Requisitos previos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- SQL Server o SQL Server Express

### 2. Configurar cadena de conexión

Abre `appsettings.json` y edita según tu entorno:

```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=IUE_Desatrasador;Trusted_Connection=True;TrustServerCertificate=True;"
```

**Opciones comunes:**
| Entorno | Connection String |
|---------|-----------------|
| SQL Express local (Windows Auth) | `Server=localhost\SQLEXPRESS;Database=IUE_Desatrasador;Trusted_Connection=True;TrustServerCertificate=True;` |
| SQL Server con usuario/contraseña | `Server=localhost;Database=IUE_Desatrasador;User Id=sa;Password=TuPassword;TrustServerCertificate=True;` |
| LocalDB (solo Visual Studio) | `Server=(localdb)\mssqllocaldb;Database=IUE_Desatrasador;Trusted_Connection=True;` |

### 3. Crear la base de datos

**Opción A — Terminal:**
```bash
dotnet ef database update
```

**Opción B — Package Manager Console (Visual Studio):**
```
Update-Database
```

### 4. Ejecutar

```bash
dotnet run
```

Abre el navegador en: http://localhost:5000

---

## 🗺️ URLs del sistema

| Vista | URL |
|-------|-----|
| Dashboard Estudiante (Juan) | http://localhost:5000/Clase/Dashboard/1 |
| Dashboard Estudiante (María) | http://localhost:5000/Clase/Dashboard/2 |
| Panel Profesor — Subir Video | http://localhost:5000/Clase/CreateVideo |
| Gestión de Excusas | http://localhost:5000/Clase/Excusas |

---

## 🔗 Configurar n8n (opcional)

En `appsettings.json` reemplaza la URL del webhook:
```json
"N8n": {
  "WebhookUrl": "https://TU_N8N.com/webhook/TU_ID"
}
```

El sistema enviará automáticamente a ese webhook un JSON con:
```json
{
  "estudianteNombre": "Juan Pérez",
  "estudianteCorreo": "juan.perez@iue.edu.co",
  "materia": "Programación I",
  "claseId": 1,
  "claseTitulo": "Introducción a C#",
  "videoUrl": "https://youtube.com/...",
  "fechaNotificacion": "2025-05-10 14:30:00"
}
```

---

## 📦 Datos de prueba incluidos

- **2 estudiantes**: Juan Pérez y María López
- **3 materias**: Programación I, Bases de Datos, Cálculo Diferencial
- **4 clases** distribuidas en las materias
- **1 video** de ejemplo en la clase "Introducción a C#"
- **Excusas**: Juan tiene aprobada para clase 1 (puede ver video), pendiente para clase 2

---

## 🛠️ Para agregar más datos

Edita `AppDbContext.cs` en el método `OnModelCreating` y agrega más registros en los bloques `HasData()`, luego ejecuta:
```bash
dotnet ef migrations add NuevosDatos
dotnet ef database update
```
