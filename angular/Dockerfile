FROM nginx:alpine
# Copy pre-built artifacts from CI build job
# Angular 20 with new builder outputs to dist/browser/ (not dist/sakai-ng/)
# Copy contents of browser folder (using . to copy all contents including hidden files)
COPY dist/sakai-ng/browser/. /usr/share/nginx/html/
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]

