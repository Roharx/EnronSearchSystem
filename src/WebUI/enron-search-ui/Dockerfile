# Use Node.js to build the Ionic application
FROM node:18-alpine AS build
WORKDIR /app

# Copy package.json and package-lock.json and install dependencies
COPY src/WebUI/enron-search-ui/package*.json ./
RUN npm install

# Copy the entire Web UI source code
COPY src/WebUI/enron-search-ui ./

# Build the application for production
RUN npm run build -- --configuration production

# Use an Nginx container to serve the Ionic app
FROM nginx:1.23-alpine
WORKDIR /usr/share/nginx/html

# Remove default Nginx static files
RUN rm -rf ./*

# Copy built Ionic app from the previous stage
COPY --from=build /app/www .

# Expose the port for Nginx
EXPOSE 80

# Start Nginx
CMD ["nginx", "-g", "daemon off;"]
