// Movie request lifecycle exercised purely through the V1 API.
//
// Unlike most of the suite these tests are self-cleaning: every request they
// create is deleted again in an afterEach, so they are safe to re-run against a
// dirty database and never leak state into other specs. Inception (27205) is
// deliberately a title no other spec touches.
describe("Movie Request V1 API tests", () => {
  const MOVIE_TMDB_ID = 27205;
  const MOVIE_TITLE = "Inception";

  let createdRequestIds: number[] = [];

  beforeEach(() => {
    cy.login();
    createdRequestIds = [];
    // Start from a known state: remove any pre-existing request for this title
    // (e.g. left over from a previous run) so the tests are fully repeatable.
    removeExistingRequests();
  });

  // Tidy up anything the test created so it can be run repeatedly.
  afterEach(() => {
    createdRequestIds.forEach((id) => {
      cy.api({
        url: `/api/v1/request/movie/${id}`,
        method: "DELETE",
        headers: authHeaders(),
        failOnStatusCode: false,
      });
    });
  });

  const authHeaders = () => ({
    Authorization: "Bearer " + window.localStorage.getItem("id_token"),
    "Content-Type": "application/json",
  });

  // Defer header construction with cy.then so the auth token is read at command
  // execution time (after cy.login has run), not while the queue is being built.
  const removeExistingRequests = () =>
    cy.then(() => {
      const headers = authHeaders();
      cy.api({
        url: `/api/v1/request/movie/search/${MOVIE_TITLE}`,
        headers,
        failOnStatusCode: false,
      }).then((res) => {
        (res.body || [])
          .filter((r: any) => r.theMovieDbId === MOVIE_TMDB_ID)
          .forEach((r: any) =>
            cy.api({
              url: `/api/v1/request/movie/${r.id}`,
              method: "DELETE",
              headers,
              failOnStatusCode: false,
            })
          );
      });
    });

  const requestMovie = (tmdbId: number) =>
    cy.api({
      url: "/api/v1/request/movie",
      method: "POST",
      body: JSON.stringify({ TheMovieDbId: tmdbId }),
      headers: authHeaders(),
      failOnStatusCode: false,
    });

  it("creates a movie request and exposes it via the request search", () => {
    requestMovie(MOVIE_TMDB_ID).then((res) => {
      expect(res.status).to.equal(200);
      expect(res.body.result, res.body.errorMessage).to.be.true;
      expect(res.body.isError).to.be.false;
      expect(res.body.requestId).to.be.a("number").and.to.be.greaterThan(0);
      expect(res.body.message).to.contain(MOVIE_TITLE);

      createdRequestIds.push(res.body.requestId);

      cy.api({
        url: `/api/v1/request/movie/search/${MOVIE_TITLE}`,
        headers: authHeaders(),
      }).then((searchRes) => {
        expect(searchRes.status).to.equal(200);
        const match = searchRes.body.find(
          (r: any) => r.theMovieDbId === MOVIE_TMDB_ID
        );
        expect(match, "requested movie should appear in the request search").to
          .not.be.undefined;
        expect(match.title).to.equal(MOVIE_TITLE);
      });
    });
  });

  it("rejects a movie that has already been requested", () => {
    requestMovie(MOVIE_TMDB_ID).then((first) => {
      expect(first.body.result, first.body.errorMessage).to.be.true;
      createdRequestIds.push(first.body.requestId);

      requestMovie(MOVIE_TMDB_ID).then((second) => {
        expect(second.body.result).to.be.false;
        expect(second.body.isError).to.be.true;
        expect(second.body.errorCode).to.equal("AlreadyRequested");
      });
    });
  });

  it("removes a movie request", () => {
    requestMovie(MOVIE_TMDB_ID).then((res) => {
      expect(res.body.result, res.body.errorMessage).to.be.true;
      const requestId = res.body.requestId;

      cy.api({
        url: `/api/v1/request/movie/${requestId}`,
        method: "DELETE",
        headers: authHeaders(),
      }).then((delRes) => {
        expect(delRes.status).to.equal(200);
      });

      cy.api({
        url: `/api/v1/request/movie/search/${MOVIE_TITLE}`,
        headers: authHeaders(),
        failOnStatusCode: false,
      }).then((searchRes) => {
        const match = (searchRes.body || []).find(
          (r: any) => r.theMovieDbId === MOVIE_TMDB_ID
        );
        expect(match, "deleted movie should not remain in the request search").to
          .be.undefined;
      });
    });
  });
});

export {};
