FROM node:22-alpine AS build

WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

ARG VITE_GATEWAY_BASE_URL=http://localhost:5203
ENV VITE_GATEWAY_BASE_URL=${VITE_GATEWAY_BASE_URL}

RUN npm run build:h5

RUN set -eu; \
    output_dir=""; \
    for dir in dist/build/h5 dist/build/web unpackage/dist/build/h5 unpackage/dist/build/web; do \
      if [ -d "$dir" ]; then \
        output_dir="$dir"; \
        break; \
      fi; \
    done; \
    test -n "$output_dir"; \
    mkdir -p /app/.output; \
    cp -r "$output_dir"/. /app/.output/

FROM nginx:1.27-alpine AS final

COPY docker/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/.output /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
