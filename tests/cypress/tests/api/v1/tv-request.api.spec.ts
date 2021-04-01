describe("TV Request V1 API tests", () => {
  beforeEach(() => {
    cy.login();
  });

  it("Request All of TV Show (Fear the Walking Dead)", () => {
    const request = {
      TvDbId: 290853,
      RequestAll: true,
    };

    cy.api({
      url: "/api/v1/request/tv",
      body: JSON.stringify(request),
      method: "POST",
      headers: {
        Authorization: "Bearer " + window.localStorage.getItem("id_token"),
        "Content-Type": "application/json",
      },
    }).then((res) => {
      expect(res.status).equal(200);
      const body = res.body;
      expect(body.result).to.be.true;
      expect(body.requestId).not.to.be.null;
      expect(body.isError).to.be.false;
      expect(body.errorMessage).to.be.null;
    });
  });

  it("Request First Season of TV Show (American Horror Story)", () => {
    const request = {
      TvDbId: 250487,
      FirstSeason: true,
    };

    cy.api({
      url: "/api/v1/request/tv",
      body: JSON.stringify(request),
      method: "POST",
      headers: {
        Authorization: "Bearer " + window.localStorage.getItem("id_token"),
        "Content-Type": "application/json",
      },
    }).then((res) => {
      expect(res.status).equal(200);
      const body = res.body;
      expect(body.result).to.be.true;
      expect(body.requestId).not.to.be.null;
      expect(body.isError).to.be.false;
      expect(body.errorMessage).to.be.null;
    });
  });

  it("Request Two Episode of First Season TV Show (The Sopranos)", () => {
    const request = {
      TvDbId: 75299,
      Seasons: [
        {
          SeasonNumber: 1,
          Episodes: [
            { EpisodeNumber: 1 },
            { EpisodeNumber: 2 },
          ],
        },
      ],
    };

    cy.api({
      url: "/api/v1/request/tv",
      body: JSON.stringify(request),
      method: "POST",
      headers: {
        Authorization: "Bearer " + window.localStorage.getItem("id_token"),
        "Content-Type": "application/json",
      },
    }).then((res) => {
      expect(res.status).equal(200);
      const body = res.body;
      expect(body.result).to.be.true;
      expect(body.requestId).not.to.be.null;
      expect(body.isError).to.be.false;
      expect(body.errorMessage).to.be.null;
    });
  });
});
