import 'package:flutter/material.dart';
import 'package:firebase_data_connect/firebase_data_connect.dart';
import 'package:flutter_app/dataconnect_generated/generated.dart';

class MoviesFutureBuilder extends StatefulWidget {
  const MoviesFutureBuilder({super.key});

  @override
  State<MoviesFutureBuilder> createState() => _MoviesFutureBuilderState();
}

class _MoviesFutureBuilderState extends State<MoviesFutureBuilder> {
  late Future<QueryResult<ListMoviesData, void>> _moviesFuture;

  @override
  void initState() {
    super.initState();
    _loadMovies();
  }

  void _loadMovies() {
    _moviesFuture = ExampleConnector.instance.listMovies().execute();
  }

  Future<void> _refresh() async {
    setState(() {
      _loadMovies();
    });
    await _moviesFuture;
  }

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<QueryResult<ListMoviesData, void>>(
      future: _moviesFuture,
      builder: (context, snapshot) {
        if (snapshot.connectionState != ConnectionState.done) {
          return const Center(child: CircularProgressIndicator());
        }

        if (snapshot.hasError) {
          return Center(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 24.0),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text('Error: ${snapshot.error}', style: const TextStyle(color: Colors.red)),
                  const SizedBox(height: 12),
                  ElevatedButton(
                    onPressed: () {
                      setState(() {
                        _loadMovies();
                      });
                    },
                    child: const Text('Retry'),
                  ),
                ],
              ),
            ),
          );
        }

        final movies = snapshot.data?.data.movies ?? [];
        if (movies.isEmpty) {
          return RefreshIndicator(
            onRefresh: _refresh,
            child: ListView(
              physics: const AlwaysScrollableScrollPhysics(),
              children: const [
                SizedBox(height: 120),
                Center(child: Text('No movies found')),
              ],
            ),
          );
        }

        return RefreshIndicator(
          onRefresh: _refresh,
          child: ListView.separated(
            physics: const AlwaysScrollableScrollPhysics(),
            itemCount: movies.length,
            separatorBuilder: (_, __) => const Divider(height: 1),
            itemBuilder: (context, index) {
              final movie = movies[index];
              final title = movie.title ?? '';
              final subtitle = <String>[
                if (movie.year != null) 'Year: ${movie.year}',
                if (movie.genre != null) 'Genre: ${movie.genre}',
              ].join(' • ');
              return ListTile(
                title: Text(title),
                subtitle: subtitle.isNotEmpty ? Text(subtitle) : null,
                leading: const Icon(Icons.movie),
                onTap: () {
                  // Expandable: navigate to details page if desired
                },
              );
            },
          ),
        );
      },
    );
  }
}
