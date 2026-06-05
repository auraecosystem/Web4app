meta:
  id: movie_list
  title: Simple movie list
  application: Example binary format for a list of movies
  file-extension: mlist
  endian: le
seq:
  - id: movies
    type: movie
    repeat: eos
types:
  movie:
    seq:
      - id: id
        type: u4
      - id: title_len
        type: u2
      - id: title
        type: str
        size: title_len
        encoding: UTF-8
      - id: year
        type: u2
      - id: genre_len
        type: u2
      - id: genre
        type: str
        size: genre_len
        encoding: UTF-8
