# Custom OOBE - Experiencia de Configuración Inicial Personalizada

Un asistente de configuración inicial (Out-of-Box Experience) completamente funcional para Windows, diseñado para proporcionar una experiencia premium al configurar un equipo nuevo.

## 🌟 Características

### Interfaz y Diseño
- **Animaciones fluidas** inspiradas en macOS con efectos de partículas flotantes
- **Temas claro y oscuro** con detección automática basada en la hora del día
- **Transiciones suaves** entre pantallas con animaciones de entrada/salida
- **Diseño moderno** con interfaz dividida: animación a la izquierda, contenido a la derecha
- **Indicador de progreso** visual en la parte inferior

### Funcionalidades Principales

#### 1. Pantallas de Bienvenida
- Mensaje de agradecimiento por la compra
- Presentación personalizada del equipo
- Animaciones secuenciales con efectos de fade-in y slide

#### 2. Creación de Usuario
- Campo de nombre de usuario con validación en tiempo real
- **Selector de avatar** con 10 opciones predefinidas coloridas
- **Opción de foto personalizada** para subir imagen propia
- Creación automática de usuario de Windows
- Configuración de foto de perfil en el sistema

#### 3. Configuración de Red
- **Detección automática** de conexión por cable Ethernet
- **Listado de redes WiFi** disponibles con intensidad de señal
- Conexión segura con contraseña en diálogo modal
- Opción de saltar si no hay internet disponible
- Verificación de conexión en tiempo real

#### 4. Instalación de Software
- **Catálogo de programas** organizados por categorías:
  - Navegadores (Chrome, Firefox, Edge)
  - Herramientas de compresión (7-Zip, WinRAR)
  - Reproductores multimedia (VLC, Windows Media Player)
  - Optimización (CCleaner)
  - Seguridad (Windows Defender, Avast)
  - Productividad (Adobe Reader, LibreOffice)
  - Comunicación (Discord, Zoom)
- **Descarga e instalación automática** con barra de progreso
- Instalaciones silenciosas en segundo plano
- Opción de saltar esta sección

#### 5. Personalización de Tema y Fondos
- **Selector de tema** (Claro/Oscuro) con aplicación inmediata
- **Galería de fondos de pantalla** con vistas previas
- Generación automática de fondos con gradientes coloridos
- Aplicación de tema en Windows completo
- Configuración de pantalla de bloqueo

#### 6. Sistema de Reseñas
- **Calificación por estrellas** (1-5 estrellas)
- Campo de comentarios opcional
- **Almacenamiento en base de datos SQLite** local
- Registro de fecha, hora, nombre de equipo y usuario

#### 7. Pantalla Final
- Mensajes de despedida con animaciones
- Marcado automático de configuración completada
- Cierre automático del asistente
- Limpieza de tareas programadas

### Seguridad y Control

#### Bloqueo de Teclas
El sistema bloquea las siguientes combinaciones para evitar salir del OOBE:
- `Win` (tecla Windows)
- `Ctrl+Alt+Del`
- `Alt+F4`
- `Ctrl+Shift+Esc` (Administrador de tareas)
- `Alt+Tab`
- `Ctrl+Tab`
- `Ctrl+W`
- Teclas F1-F12 (excepto F5)

#### Protecciones Adicionales
- **Cierre automático del Administrador de tareas** si se abre
- Ventana en pantalla completa sin bordes
- Topmost (siempre visible)
- Sin mostrar en barra de tareas

### Auto-Inicio
- Ejecución automática antes del login de Windows
- Configuración mediante tarea programada de Windows
- Entrada en registro RunOnce
- Permisos de administrador para operaciones del sistema

## 📋 Requisitos

- Windows 10/11 (64-bit)
- .NET 8.0 SDK o superior
- Permisos de Administrador
- Visual C++ Redistributable 2015-2022
- 2 GB de espacio en disco (para instalaciones)

## 🚀 Instalación y Compilación

### Paso 1: Instalar Dependencias

Abre PowerShell como **Administrador** y ejecuta:

```powershell
cd ruta\al\proyecto
.\install-dependencies.ps1
```

Este script instalará automáticamente:
- .NET SDK 8.0
- Visual C++ Redistributable
- Paquetes NuGet necesarios
- Configuración de Windows Features
- Reglas de firewall

### Paso 2: Compilar el Proyecto

```powershell
.\build.ps1
```

Este script:
- Limpia compilaciones anteriores
- Restaura paquetes NuGet
- Compila el proyecto en modo Release
- Crea un ejecutable autónomo en `Build\CustomOOBE.exe`

### Paso 3: Desplegar en el Sistema

```powershell
.\deploy.ps1 -ComputerName "Mi Equipo Premium"
```

Este script:
- Copia archivos a `C:\Program Files\CustomOOBE`
- Crea configuración en `C:\ProgramData\CustomOOBE`
- Configura auto-inicio en el registro
- Crea tarea programada para ejecución antes del login
- Establece permisos de archivos

## 🎯 Uso

### Ejecución Automática
Después del despliegue, el OOBE se ejecutará automáticamente en el próximo reinicio del sistema, **antes** de la pantalla de inicio de sesión de Windows.

### Ejecución Manual (para pruebas)
```powershell
cd "C:\Program Files\CustomOOBE"
.\CustomOOBE.exe /oobe
```

### Deshabilitar el Auto-Inicio
Si necesitas deshabilitar el OOBE temporalmente:

```powershell
# Eliminar tarea programada
schtasks /Delete /TN "CustomOOBE" /F

# Eliminar entrada del registro
reg delete "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce" /v CustomOOBE /f
```

## 📁 Estructura del Proyecto

```
CustomOOBE/
├── Assets/
│   ├── Avatars/          # Avatares de usuario predefinidos
│   ├── Wallpapers/       # Fondos de pantalla
│   └── LockScreens/      # Pantallas de bloqueo
├── Models/               # Modelos de datos
│   └── OOBEConfig.cs
├── Services/             # Servicios del sistema
│   ├── WiFiService.cs
│   ├── UserService.cs
│   ├── SoftwareService.cs
│   ├── ThemeService.cs
│   ├── DatabaseService.cs
│   └── KeyboardBlocker.cs
├── Views/                # Pantallas del OOBE
│   ├── WelcomePage.xaml
│   ├── UserSetupPage.xaml
│   ├── NetworkSetupPage.xaml
│   ├── SoftwareSetupPage.xaml
│   ├── ThemeSetupPage.xaml
│   ├── ReviewPage.xaml
│   └── FinalPage.xaml
├── Themes/               # Temas claro y oscuro
│   ├── LightTheme.xaml
│   └── DarkTheme.xaml
├── App.xaml              # Aplicación principal
├── MainWindow.xaml       # Ventana principal con animaciones
└── CustomOOBE.csproj     # Archivo de proyecto

Scripts:
├── install-dependencies.ps1  # Instalación de dependencias
├── build.ps1                 # Compilación del proyecto
└── deploy.ps1                # Despliegue y configuración
```

## 🔧 Personalización

### Cambiar el Nombre del Equipo
Edita el archivo de configuración o pasa el parámetro al deploy:

```powershell
.\deploy.ps1 -ComputerName "Mi Marca Premium Edition"
```

### Agregar Más Software
Edita `Services/SoftwareService.cs` y agrega nuevos paquetes al método `GetAvailableSoftware()`:

```csharp
new SoftwarePackage
{
    Name = "Tu Programa",
    Description = "Descripción del programa",
    Category = "Categoría",
    DownloadUrl = "https://url-descarga.com/installer.exe",
    SizeInMB = 100
}
```

### Personalizar Avatares
Reemplaza las imágenes en `Assets/Avatars/` con tus propias imágenes (formato PNG, 200x200 px recomendado).

### Personalizar Fondos de Pantalla
Agrega imágenes en `Assets/Wallpapers/` (formato PNG/JPG, resolución 1920x1080 recomendado).

### Modificar Temas
Edita los archivos en `Themes/`:
- `LightTheme.xaml` para el tema claro
- `DarkTheme.xaml` para el tema oscuro

### Cambiar la Hora de Detección de Tema
Edita `App.xaml.cs`, línea:

```csharp
var isDarkMode = currentHour >= 18 || currentHour < 6; // 6 PM a 6 AM = oscuro
```

## 📊 Base de Datos de Reseñas

Las reseñas se almacenan en:
```
C:\ProgramData\CustomOOBE\reviews.db
```

Para leer las reseñas, puedes usar cualquier herramienta SQLite o ejecutar:

```powershell
# Instalar módulo SQLite para PowerShell
Install-Module -Name PSSQLite

# Leer reseñas
$reviews = Invoke-SqliteQuery -DataSource "C:\ProgramData\CustomOOBE\reviews.db" -Query "SELECT * FROM Reviews"
$reviews | Format-Table
```

## ⚠️ Notas Importantes

1. **Permisos de Administrador**: El OOBE requiere permisos elevados para crear usuarios, conectar a WiFi y modificar configuraciones del sistema.

2. **Primera Ejecución**: El OOBE está diseñado para ejecutarse una sola vez. Después de completarse, se marca como completado y no se volverá a ejecutar.

3. **Compatibilidad**: Probado en Windows 10 (versión 2004+) y Windows 11. Algunas funcionalidades pueden variar según la versión.

4. **Seguridad**:
   - Las contraseñas WiFi se almacenan de forma segura en Windows
   - El bloqueo de teclas es reversible (el sistema puede desbloquearse desde el código)
   - No se recopila información personal sin consentimiento

5. **Instalación de Software**:
   - Requiere conexión a internet activa
   - Las URLs de descarga pueden cambiar con el tiempo
   - Algunos instaladores requieren confirmación manual

## 🐛 Solución de Problemas

### El OOBE no se ejecuta automáticamente
- Verifica que la tarea programada existe: `schtasks /Query /TN CustomOOBE`
- Revisa los permisos del ejecutable
- Comprueba los logs de eventos de Windows

### Error al crear usuario
- Asegúrate de que el nombre no contenga caracteres especiales
- Verifica que el usuario no exista previamente
- Confirma permisos de administrador

### No se conecta a WiFi
- Verifica que el adaptador WiFi esté habilitado
- Confirma que la contraseña es correcta
- Algunos adaptadores requieren drivers específicos

### Instalación de software falla
- Verifica conexión a internet
- Algunas URLs de descarga pueden haber cambiado
- Revisa si el antivirus está bloqueando descargas

### El tema no se aplica correctamente
- Reinicia el Explorador de Windows: `taskkill /f /im explorer.exe && start explorer.exe`
- Verifica permisos en el registro
- Algunos temas requieren reinicio completo

## 📝 Licencia

Este proyecto es de código abierto para uso educativo y comercial. Siéntete libre de modificarlo según tus necesidades.

## 🤝 Contribuciones

¡Las contribuciones son bienvenidas! Si encuentras bugs o tienes sugerencias:

1. Reporta el issue detallando el problema
2. Propón mejoras o nuevas características
3. Envía pull requests con mejoras

## 📧 Soporte

Para soporte técnico o preguntas, crea un issue en el repositorio del proyecto.

---

**¡Disfruta de tu experiencia OOBE personalizada!** 🎉
