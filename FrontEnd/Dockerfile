# ====================================================================
# Dockerfile - Frontend ULTRA SIMPLES
# Remove comandos problemáticos
# ====================================================================

FROM node:18-alpine AS build
WORKDIR /app

# Copiar tudo
COPY . .

# Instalar e buildar
RUN npm install --force
RUN npm run build || npm run build -- --configuration=production

FROM nginx:1.25-alpine
COPY --from=build /app/dist/book-manager-frontend /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]