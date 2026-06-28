#!/bin/zsh

echo "============================================="
echo "   🚀 Votify Demo Lanzador (HTTP) 🚀"
echo "============================================="

# 1. Obtener la IP local de la Wi-Fi
IP=$(ipconfig getifaddr en0)

if [ -z "$IP" ]; then
    echo "❌ No se pudo detectar la IP automáticamente."
    echo "👉 Por favor, introduce tu IP local manualmente (ej: 192.168.1.50):"
    read IP
fi

echo "✅ IP detectada: $IP"

# Usamos los puertos HTTP para evitar problemas de certificados con iOS/Safari
API_DIR="./API"
WEB_DIR="./Web"
API_PORT="5059"
WEB_PORT="5113"
APPSETTINGS="$WEB_DIR/wwwroot/appsettings.json"

NUEVA_URL="http://$IP:$API_PORT/"
LOCALHOST_URL="https://localhost:7185/" # Restauramos al original con HTTPS para el desarrollo normal

# Función para restaurar y limpiar al presionar Ctrl+C
cleanup() {
    echo ""
    echo "🛑 Deteniendo servidores..."
    kill -TERM $API_PID 2>/dev/null
    kill -TERM $WEB_PID 2>/dev/null
    
    echo "🔄 Restaurando appsettings.json a localhost..."
    sed -i '' -E 's|"ApiBaseUrl":[[:space:]]*"[^"]*"|"ApiBaseUrl": "'"$LOCALHOST_URL"'"|' "$APPSETTINGS"
    
    echo "✅ Entorno restaurado correctamente. ¡Hasta luego!"
    exit 0
}

# Capturar Ctrl+C
trap cleanup SIGINT SIGTERM

echo "📝 Actualizando appsettings.json para apuntar a $NUEVA_URL ..."
sed -i '' -E 's|"ApiBaseUrl":[[:space:]]*"[^"]*"|"ApiBaseUrl": "'"$NUEVA_URL"'"|' "$APPSETTINGS"

echo "⏳ Compilando y levantando la API (esto puede tardar unos segundos)..."
cd "$API_DIR"
dotnet run --urls "http://0.0.0.0:$API_PORT" &
API_PID=$!
cd ..

# Darle unos segundos a la API para asegurar que se compila y levanta primero
sleep 8

echo "⏳ Levantando la Web en http://0.0.0.0:$WEB_PORT ..."
cd "$WEB_DIR"
dotnet run --urls "http://0.0.0.0:$WEB_PORT" &
WEB_PID=$!
cd ..

echo ""
echo "==========================================================="
echo "✅ ¡Todo en marcha!"
echo ""
echo "📱 El profesor (o tú en el iPhone) debe entrar a:"
echo "👉 http://$IP:$WEB_PORT"
echo ""
echo "⚠️ Al usar HTTP no tendrás errores de certificado en el móvil"
echo "   y la página cargará perfectamente a la primera."
echo ""
echo "👉 Para apagar todo, presiona Ctrl + C en esta terminal."
echo "==========================================================="

wait
