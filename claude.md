# Instrucciones para Claude Code - Proyecto AsistCC

Este archivo contiene instrucciones permanentes para Claude Code sobre cómo mantener la documentación del proyecto actualizada durante el desarrollo.

## 📝 Política de Documentación

Cada vez que implementes una nueva funcionalidad significativa o realices cambios importantes en el proyecto, DEBES actualizar automáticamente los siguientes archivos:

### 1. resumenDesarrollo.txt
**Ubicación**: `/resumenDesarrollo.txt`

**Cuándo actualizar**:
- Después de implementar una nueva funcionalidad completa
- Al completar una historia de usuario o tarea
- Después de hacer cambios arquitectónicos significativos
- Al finalizar una sesión de desarrollo importante

**Qué incluir**:
- Fecha de la implementación
- Branch en el que se trabajó
- Descripción detallada de lo implementado
- Lista de archivos creados con su propósito
- Lista de archivos modificados con los cambios realizados
- Endpoints creados o modificados (si aplica)
- Cambios en la base de datos (si aplica)
- Decisiones de diseño importantes tomadas
- Tecnologías o librerías nuevas utilizadas
- Validaciones implementadas
- Flujo de usuario (si aplica)
- Próximos pasos o TODOs pendientes
- Notas importantes para el desarrollador

**Formato**:
```
================================================================================
IMPLEMENTACIÓN: [Título de la funcionalidad]
================================================================================
Fecha: YYYY-MM-DD
Branch: [nombre-del-branch]

DESCRIPCIÓN:
[Descripción detallada de lo implementado]

ARCHIVOS CREADOS:
1. Ruta/al/archivo.cs
   - Propósito y funcionalidad

ARCHIVOS MODIFICADOS:
1. Ruta/al/archivo.cs
   - Cambios realizados

[... resto de secciones según corresponda ...]

================================================================================
```

### 2. README.md
**Ubicación**: `/README.md`

**Cuándo actualizar**:
- Al completar una funcionalidad visible para el usuario final
- Al añadir nuevos endpoints a la API
- Al implementar nuevas secciones de la aplicación
- Al cambiar tecnologías o dependencias principales
- Al añadir nuevos comandos o instrucciones de ejecución

**Secciones que deben mantenerse actualizadas**:

#### ✨ Funcionalidades Implementadas
Añadir nuevas funcionalidades bajo las categorías apropiadas:
- 🔐 Autenticación y Registro
- 👥 Gestión de [Entidad]
- 💰 Cuentas Corrientes
- 📊 Movimientos
- [Crear nuevas categorías según sea necesario]

**Formato para nuevas funcionalidades**:
```markdown
### 🎯 [Nombre de la Categoría]

#### [Nombre de la Funcionalidad]
- Punto destacado 1
- Punto destacado 2
- Característica importante 3
```

#### 🌐 Endpoints API
Actualizar la lista de endpoints cuando se crean nuevos:
```markdown
### [Categoría de Endpoints]
```
[MÉTODO] /ruta/del/endpoint # Descripción breve
```
```

#### 📦 Estructura del Proyecto
Actualizar si se añaden nuevos proyectos o carpetas importantes

#### 🚀 Cómo Ejecutar
Actualizar si hay nuevos pasos de configuración o comandos

#### 🎯 Próximas Funcionalidades
- Marcar como completadas [✅] las funcionalidades implementadas
- Añadir nuevas funcionalidades planeadas

#### Actualizar metadatos al final del README
```markdown
**Última actualización**: YYYY-MM-DD
**Branch actual**: [nombre-del-branch]
**Versión**: X.Y.Z-[estado]
```

## 🔄 Proceso Automático

### Cuando completes una tarea o funcionalidad:

1. **Identificar el alcance**:
   - ¿Es una nueva funcionalidad completa? → Actualizar AMBOS archivos
   - ¿Es un cambio interno sin impacto visual? → Actualizar solo resumenDesarrollo.txt
   - ¿Es un bugfix menor? → No requiere actualización (a menos que sea significativo)

2. **Actualizar resumenDesarrollo.txt**:
   - Añadir una nueva sección al final del archivo
   - Incluir toda la información relevante
   - Ser detallado y específico

3. **Actualizar README.md**:
   - Ubicar la sección apropiada
   - Añadir la nueva funcionalidad de forma concisa
   - Actualizar endpoints si aplica
   - Actualizar metadatos (fecha, versión)
   - Si la funcionalidad estaba en "Próximas Funcionalidades", marcarla como completada [✅]

4. **Comunicar al usuario**:
   - Informar al usuario que los archivos de documentación fueron actualizados
   - Proporcionar un resumen breve de qué se documentó

## 🎯 Ejemplos de Qué Documentar

### ✅ SÍ documentar:
- Nueva página o componente de UI
- Nuevo endpoint de API
- Nueva entidad en la base de datos
- Sistema de autenticación/autorización
- Integración con servicios externos
- Cambios en el flujo de usuario
- Nuevas validaciones importantes
- Cambios en la arquitectura
- Nuevas dependencias o librerías
- Migraciones de base de datos

### ❌ NO documentar (o documentar brevemente):
- Corrección de typos
- Ajustes menores de CSS
- Refactoring sin cambio de funcionalidad
- Cambios de formato de código
- Comentarios añadidos
- Logs añadidos

## 📋 Checklist Pre-Commit

Antes de considerar una tarea como completada, verifica:

- [ ] ¿La funcionalidad está completa y testeada?
- [ ] ¿Se crearon o modificaron archivos importantes?
- [ ] ¿El usuario final notará este cambio?
- [ ] ¿Se añadieron endpoints nuevos?
- [ ] Si respondiste SÍ a alguna pregunta → Actualizar documentación

## 🚨 Recordatorios Importantes

1. **SIEMPRE actualizar la fecha** en los metadatos
2. **MANTENER el formato consistente** con las secciones existentes
3. **SER ESPECÍFICO** en los archivos creados/modificados
4. **INCLUIR ejemplos** cuando sea relevante (especialmente para endpoints)
5. **DOCUMENTAR las decisiones de diseño** importantes (ej: "Por qué dos botones en lugar de uno")
6. **ACTUALIZAR los TODOs** cuando se completen funcionalidades planeadas

## 💡 Tips para Buena Documentación

- **Piensa en el desarrollador futuro**: Escribe como si alguien más fuera a retomar el proyecto en 6 meses
- **Sé conciso pero completo**: Balance entre detalle y legibilidad
- **Usa ejemplos**: Especialmente para APIs y flujos de usuario
- **Mantén la consistencia**: Sigue el formato existente
- **Documenta el "por qué"**: No solo el "qué", sino también las razones detrás de las decisiones

## 🔍 Verificación de Calidad

Antes de terminar la sesión, pregúntate:
- ¿Otro desarrollador podría entender lo que implementé leyendo la documentación?
- ¿Incluí todos los archivos nuevos/modificados importantes?
- ¿Documenté los endpoints con ejemplos?
- ¿Expliqué las decisiones de diseño no obvias?
- ¿Actualicé los metadatos (fecha, versión, branch)?

---

## 🎓 Ejemplo de Flujo Completo

**Tarea**: Implementar sistema de notificaciones por email

**Después de implementar**:

1. ✅ Actualizar `resumenDesarrollo.txt`:
   ```
   ================================================================================
   IMPLEMENTACIÓN: Sistema de Notificaciones por Email
   ================================================================================
   Fecha: 2025-11-13
   Branch: feature/email-notifications

   DESCRIPCIÓN:
   Se implementó un sistema completo de notificaciones...

   ARCHIVOS CREADOS:
   1. SaasACC.Application/Services/EmailService.cs
      - Servicio para envío de emails usando SendGrid
   ...
   ```

2. ✅ Actualizar `README.md`:
   ```markdown
   ### 📧 Notificaciones

   #### Sistema de Email
   - Envío de notificaciones de movimientos
   - Alertas de límite de crédito
   - Confirmación de registro
   - Integración con SendGrid
   ```

3. ✅ Actualizar endpoints si aplica:
   ```markdown
   ### Notificaciones
   ```
POST /api/notifications/send-email # Enviar email
GET  /api/notifications/preferences # Obtener preferencias
   ```
   ```

4. ✅ Actualizar metadatos:
   ```markdown
   **Última actualización**: 2025-11-13
   **Branch actual**: feature/email-notifications
   **Versión**: 1.1.0-beta
   ```

5. ✅ Informar al usuario:
   "He actualizado la documentación del proyecto:
   - resumenDesarrollo.txt: Añadida sección detallada sobre el sistema de emails
   - README.md: Actualizada sección de funcionalidades y endpoints"

---

**Recuerda**: La documentación es tan importante como el código. Un proyecto bien documentado es un proyecto mantenible.
