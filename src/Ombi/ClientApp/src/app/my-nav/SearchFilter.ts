
export class SearchFilter {
  movies: boolean;
  tvShows: boolean;
  music: boolean;
  people: boolean;
  public constructor(init?:Partial<SearchFilter>) {
    Object.assign(this, init);
}
}
