export class SearchFilter {
  movies: boolean;
  tvShows: boolean;
  music: boolean;
  people: boolean;
  public constructor(init?:Partial<SearchFilter>) {
    Object.assign(this, init);
}
}

export const DEFAULT_SEARCH_FILTER: SearchFilter = {
  movies: true,
  music: false,
  people: false,
  tvShows: true
};