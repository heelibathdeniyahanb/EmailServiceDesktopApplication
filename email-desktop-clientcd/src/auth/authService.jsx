let token = null;

export const authService = {
  setToken: (jwt) => {
    token = jwt;
  },
  getToken: () => token,
  clearToken: () => {
    token = null;
  }
};
