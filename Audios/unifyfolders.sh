#!/bin/bash

mkdir files

for folder in $(ls)
do
	if [[ $folder = 'files' ]]
	then
		continue
	fi
	
	if [[ -d $folder ]]
	then
		mv $folder/*.wav ./files
		rm $folder/*
		#rmdir $folder
	fi
done
