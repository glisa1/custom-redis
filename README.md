# custom-redis

The app will listen on port 6379 (hardcoded). 
In order for the container to be able to receive requests from host machine the host address for httpListener is `http:\\*:6379\`. But when trying to run it locally you will get the `Access denied` error. So in order for it to work run with admin priviledges or change host address in config file.

To run the container from the image use this command:
`docker run -dt -p <host_port>:6379 --name <container_name> <image_id>`
