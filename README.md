# Bodeglook

Red social gastronómica argentina. Organizás salidas con amigos (planes con votación), descubrís restaurantes y bodegas y dejás reseñas honestas, sin canjes.

## Integrantes

- Uriel Chernomoretz
- Santino Adobatto
- Pedro Aksarlian
- Iván Esquenazi Korman
- Matías Duvidovich

## Descripción del proyecto

Bodeglook resuelve el "¿adónde vamos?" de un grupo de amigos. Dentro de un grupo de chat, cualquiera arma un plan con 2 o 3 lugares y varios horarios. El grupo vota y el creador confirma el lugar y el horario ganadores. Además, cada persona puede buscar lugares en el mapa, guardar favoritos y dejar reseñas que se publican 24 horas después de la visita. La app tiene en cuenta las restricciones alimentarias de cada uno (sin TACC, vegano, kosher, etc.) para recomendar solo lo que es apto.

## Funcionalidades

- **Cuenta:** registro, inicio y cierre de sesión. Las contraseñas se guardan con hash (PBKDF2).
- **Onboarding:** gustos, restricciones alimentarias, zonas, con quién sale y filtro "Solo lo apto para mí".
- **Inicio:** recomendados para vos y tus favoritos. Se filtran por restricción sin recargar la página.
- **Descubrir:** buscador con mapa, filtros por barrio y categoría, orden por cercanía o puntaje y búsquedas recientes. Si un lugar no está en la base, se busca en Geoapify y se guarda.
- **Detalle del restaurante:** información, galería de fotos, favoritos y reseñas de la comunidad.
- **Reseñas:** puntaje de comida, ambiente, atención y precio, comentario y foto opcional. El servidor no deja publicar comentarios con palabras ofensivas, aunque estén escritas con mayúsculas, tildes, letras repetidas, números o puntos en el medio.
- **Chats de grupo:** mensajes en tiempo casi real, creación de grupos con foto.
- **Planes:** crear un plan desde el chat, votar lugar y horario, y que el creador confirme o cancele. Si hay empate, el creador elige entre los empatados.
- **Perfil:** datos, restricciones, favoritos y planes. Se pueden editar el perfil y la foto, con recorte.
- **Registro de restaurantes:** plan Gratis o Pro.



## Cómo ejecutar el proyecto

1. **Requisitos:** .NET 9 SDK y SQL Server (Express).
2. **Base de datos:** en SQL Server Management Studio, ejecutar `Bodeglook.sql` (crea la base con las tablas, los procedimientos, los datos de prueba y las palabras que no se permiten en las reseñas).
3. **Conexión:** revisar la connection string `ConnectionStrings:Bodeglook` en `appsettings.json`. Por defecto usa `Server=localhost` con autenticación de Windows.
4. **Geoapify (opcional):** sirve para buscar lugares que no están en la base. Guardar la API key en los user-secrets:
   ```
   dotnet user-secrets set "Geoapify:ApiKey" "TU_API_KEY"
   ```
   Sin la key, la app funciona igual con los lugares que ya están cargados.
5. **Ejecutar:**
   ```
   dotnet run
   ```
   Abrir http://localhost:5168
6. **Usuario de prueba:** `camila.fernandez@example.com`, contraseña `Bodeglook2026`. Todos los usuarios de prueba tienen la misma contraseña.

## Enlaces

- **Figma:**
  - [UI Kit](https://www.figma.com/design/s1xP4urCzZ6f4WrvJ0EMY3/Bodeglook?node-id=623-213)
  - [Prototipo en alta](https://www.figma.com/design/s1xP4urCzZ6f4WrvJ0EMY3/Bodeglook?node-id=5644-147)
- **Miro:** https://miro.com/app/board/uXjVHVZVZhY=/
- **Trello:** https://trello.com/b/dowjZTTI/bodeglook
- **Repositorio:** https://github.com/Uri-Cherno444/Bodeglook
