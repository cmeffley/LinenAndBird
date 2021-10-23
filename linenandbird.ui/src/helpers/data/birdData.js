import axios from 'axios';
import config from '../Config';

const getAllBirds = () => new Promise((resolve, reject) => {
  axios.get(`${config.dbUrl}/api/birds`)
  .then(response => resolve(response.data))
  .catch(error => reject(error));
});

export default getAllBirds;
