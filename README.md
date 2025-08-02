# YouTube Downloader GUI (C# + Windows Forms)

Una interfaz gráfica súper simple para `yt-dlp`, creada como un experimento de *vibe coding* 🧪🎧. Este proyecto fue hecho con amor y cafeína, ideal para quienes quieren descargar videos sin abrir una terminal.

---

## 🎯 ¿Qué hace esta app?

- Descarga videos desde YouTube usando `yt-dlp`.
- Muestra el progreso de descarga.
- Permite seleccionar formato de video o convertir directamente a audio.
- Provee una experiencia sencilla y directa desde una GUI minimalista.

---

## 🛠️ Requisitos

- [.NET 6.0 SDK o superior](https://dotnet.microsoft.com/en-us/download)
- `yt-dlp.exe` en la misma carpeta que el `.exe` de esta app
- Opcionalmente, para conversiones de video a audio:
  - `ffmpeg.exe` debe estar en la misma carpeta (o en el `PATH` del sistema)

---

## 🚀 Cómo usar

1. Descarga los binarios:
   - `yt-dlp.exe` desde [https://github.com/yt-dlp/yt-dlp/releases](https://github.com/yt-dlp/yt-dlp/releases)
   - `ffmpeg.exe` desde [https://ffmpeg.org/download.html](https://ffmpeg.org/download.html)

2. Coloca `yt-dlp.exe` (y opcionalmente `ffmpeg.exe`) en la misma carpeta que `YouTubeDownloaderForm.exe`.

3. Ejecuta la app y pega el enlace de YouTube.

4. Haz clic en "Descargar" y disfruta. 🎬

---

## ⚠️ Bugs conocidos

> Este proyecto fue hecho como un experimento, por lo tanto tiene algunos detalles por pulir:

- Detener una descarga a mitad puede causar errores o dejar procesos colgados.
- La interfaz es básica y no tiene feedback avanzado (por ejemplo, errores detallados o cola de descargas).

👉 A pesar de esto, **la funcionalidad principal funciona perfectamente**: descarga el video correctamente, que era el objetivo esencial del proyecto.

---

## 📁 Estructura del proyecto

- `YouTubeDownloaderForm.cs`: Lógica principal de la interfaz.
- `yt-dlp.exe`: Motor de descarga.
- `ffmpeg.exe` *(opcional)*: Soporte para conversión de formatos.

---

## 🧪 ¿Por qué esto existe?

Fue una tarde de inspiración, algo de música lo-fi, y muchas ganas de jugar con `yt-dlp` desde un entorno visual sin complicarse con CLI. ¿La meta? Que funcione, aunque no sea perfecto.

---

## 📜 Licencia

MIT. Usa, rompe, mejora, y si te gusta, [¡invita un café! ☕](https://tppay.me/mdtqpq20)

---

