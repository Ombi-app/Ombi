describe("TV Search V1 API tests", () => {
  beforeEach(() => {
    cy.login();
  });

  it("Get Extra TV Info", () => {
    cy.api({
      url: "/api/v1/search/tv/info/287247",
      headers: {
        Authorization: "Bearer " + window.localStorage.getItem("id_token"),
      },
    }).then((res) => {
      expect(res.status).equal(200);
      const body = res.body;
      cy.fixture("api/v1/tv-search-extra-info").then((x) => {
        expect(res.body.title).equal(x.title);
        expect(res.body.status).equal(x.status);
        expect(res.body.id).equal(x.id);
        expect(res.body.firstAired).equal(x.firstAired);
        expect(res.body.network).equal(x.network);
        expect(res.body.seriesId).equal(x.seriesId);
        expect(res.body.runtime).equal(x.runtime);
        expect(res.body.networkId).equal(x.networkId);
        expect(res.body.overview).equal(x.overview);
        expect(res.body.seasonRequests.length).equal(x.seasonRequests.length);
        expect(res.body.seasonRequests[0].seasonNumber).equal(
          x.seasonRequests[0].seasonNumber
        );
        expect(res.body.seasonRequests[0].episodes.length).equal(
          x.seasonRequests[0].episodes.length
        );
        expect(res.body.seasonRequests[0].episodes[0].episodeNumber).equal(
          x.seasonRequests[0].episodes[0].episodeNumber
        );
        expect(res.body.seasonRequests[0].episodes[0].title).equal(
          x.seasonRequests[0].episodes[0].title
        );
        expect(res.body.seasonRequests[0].episodes[0].airDate).equal(
          x.seasonRequests[0].episodes[0].airDate
        );
        expect(res.body.seasonRequests[0].episodes[0].url).equal(
          x.seasonRequests[0].episodes[0].url
        );
        expect(res.body.seasonRequests[0].episodes[0].available).equal(
          x.seasonRequests[0].episodes[0].available
        );
        expect(res.body.seasonRequests[0].episodes[0].requested).equal(
          x.seasonRequests[0].episodes[0].requested
        );
        expect(res.body.seasonRequests[0].episodes[0].approved).equal(
          x.seasonRequests[0].episodes[0].approved
        );
        expect(res.body.seasonRequests[0].episodes[0].airDateDisplay).equal(
          x.seasonRequests[0].episodes[0].airDateDisplay
        );
        expect(res.body.seasonRequests[0].seasonAvailable).equal(
          x.seasonRequests[0].seasonAvailable
        );
        expect(res.body.id).equal(x.id);
        expect(res.body.imdbId).equal(x.imdbId);
        expect(res.body.theMovieDbId).equal(x.theMovieDbId);
      });
    });
  });

  it("TV Basic Search", () => {
    cy.api({
      url: "/api/v1/search/tv/Shitts Creek",
      headers: {
        Authorization: "Bearer " + window.localStorage.getItem("id_token"),
      },
    }).then((res) => {
      expect(res.status).equal(200);
      const body = res.body;
      expect(body[0].title).is.equal("Schitt's Creek");
      expect(body[0].status).is.equal("Ended");
      expect(body[0].id).is.not.null;
      expect(body[0].id).to.be.an("number");
    });
  });

  const types = ["popular", "trending", "anticipated", "mostwatched"];

  types.forEach((type) => {
    // derive test name from data
    it(`${type} TV List`, () => {
      cy.api({
        url: "/api/v1/search/tv/" + type,
        headers: {
          Authorization: "Bearer " + window.localStorage.getItem("id_token"),
        },
      }).then((res) => {
        expect(res.status).equal(200);
        const body = res.body;
        expect(body.length).is.greaterThan(0);
        expect(body[0].title).is.not.null;
        expect(body[0].id).is.not.null;
        expect(body[0].id).to.be.an("number");
      });
    });
  });
});
