from clint.textui import progress
from bs4 import BeautifulSoup
from urllib import request
import requests, re


class Downloader:

	def __init__(self):
		self.url = None
		self.vid_id = None
		self.key = None
		self.filename = None
		self.video_list = []

	def get_params(self):
		html = request.urlopen(self.url)
		soup = BeautifulSoup(html, "html.parser")
		self.filename = re.sub("V LIVE - ", "", soup.find("title").text)
		# Jeez ;-;
		tmp = str(soup.prettify())
		n = 0
		for i in tmp.splitlines():
			if n == 2:
				key = re.sub("\"", "", i)
				self.key = re.sub(",", "", key).strip()
				break
			if n == 1:
				v_id = re.sub("\"", "", i)
				self.vid_id = re.sub(",", "", v_id).strip()
				n += 1
			if re.match(r'.*video.init.*', i):
				n +=1

	def retrieve_videos(self):
		url = "http://global.apis.naver.com/rmcnmv/rmcnmv/vod_play_videoInfo.json"

		response = requests.get(url, params={"videoId" : self.vid_id, 
			"key" : self.key,
			"ptc" : "http",
			"doct": "json",
			"cpt" : "vtt"})

		video_list = response.json()["videos"]["list"]
		for video in video_list:
			filename = video["encodingOption"]["name"]
			video_source = video["source"]
			self.video_list.append((filename, video_source))

	def download(self):
		while True:
			print("Found the following video qualities:")
			for i in range(0, len(self.video_list)):
				q, l = self.video_list[i]
				print(str(i) + ". " + q)
			dwn = input("Please choose the number you wish to download.\n")
			if int(dwn) < len(self.video_list):
				filename, link = self.video_list[int(dwn)]
				# Progress bar source - https://stackoverflow.com/questions/15644964/python-progress-bar-and-downloads
				r = requests.get(link, stream=True)
				with open(self.filename + "[" + filename + "]" ".mp4", 'wb') as f:
				    total_length = int(r.headers.get('content-length'))
				    for chunk in progress.bar(r.iter_content(chunk_size=1024), expected_size=(total_length/1024) + 1): 
				        if chunk:
				            f.write(chunk)
				break
		print("Successfully finished downloading " + self.filename +".\n")

	def run(self, url):
		self.url = url
		self.get_params()
		self.retrieve_videos()
		self.download()
		self.video_list = []

if __name__ == "__main__":
	downloader = Downloader()
	while True:
		cmd = input("Please paste in the URL you wish to download. Or, type 'exit' to close\n")
		if cmd == "exit":
			break
		downloader.run(cmd)

	

