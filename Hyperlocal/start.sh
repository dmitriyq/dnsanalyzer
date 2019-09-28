#!/bin/bash

kresd -f 1 & kresd -f 2 & kresd -f 3 & kresd -f 4

exec "$@"