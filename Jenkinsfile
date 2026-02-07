pipeline {
    agent any
    
    tools {
        dotnet 'dotnet-9.0'
    }
    
    environment {
        SONAR_HOST_URL = 'http://localhost:9000'
        SONAR_SCANNER_HOME = tool 'sonar-scanner'
        DOCKER_REGISTRY = 'localhost:5000'
        IMAGE_NAME = 'voting-app'
        IMAGE_TAG = "${env.BUILD_ID}"
    }
    
    stages {
        stage('Checkout') {
            steps {
                git branch: 'main',
                    url: 'https://github.com/atechsphere/sample-voting-dotnet-app-v1.0.git'
            }
        }
        
        stage('Restore & Build') {
            steps {
                dir('src') {
                    sh 'dotnet restore VotingAppSolution.sln'
                    sh 'dotnet build VotingAppSolution.sln --configuration Release --no-restore'
                }
            }
        }
        
        stage('Run Tests with Coverage') {
            steps {
                dir('src') {
                    sh '''
                        dotnet test VotingApp.Tests/VotingApp.Tests.csproj \
                          --configuration Release \
                          --no-build \
                          --logger "trx;LogFileName=test-results.trx" \
                          --collect:"XPlat Code Coverage" \
                          --settings coverlet.runsettings
                    '''
                    
                    // Process coverage reports
                    sh '''
                        reportgenerator \
                          -reports:**/coverage.cobertura.xml \
                          -targetdir:./TestResults/CoverageReport \
                          -reporttypes:HtmlInline
                    '''
                }
            }
            
            post {
                always {
                    junit 'src/**/TestResults/*.trx'
                    publishHTML(target: [
                        reportDir: 'src/TestResults/CoverageReport',
                        reportFiles: 'index.html',
                        reportName: 'Code Coverage Report'
                    ])
                }
            }
        }
        
        stage('SonarQube Analysis') {
            steps {
                dir('src') {
                    withSonarQubeEnv('SonarQube-Local') {
                        sh '''
                            dotnet sonarscanner begin \
                              /k:"VotingApp" \
                              /n:"Voting Application" \
                              /v:"${BUILD_ID}" \
                              /d:sonar.cs.vstest.reportsPaths="**/TestResults/*.trx" \
                              /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" \
                              /d:sonar.coverage.exclusions="**Tests*.cs,**/Migrations/**" \
                              /d:sonar.exclusions="**/wwwroot/lib/**,**/node_modules/**"
                            
                            dotnet build VotingAppSolution.sln --configuration Release
                            
                            dotnet sonarscanner end
                        '''
                    }
                }
            }
        }
        
        stage('Build Docker Image') {
            steps {
                script {
                    docker.build("${DOCKER_REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}", "--target runtime .")
                }
            }
        }
        
        stage('Push to Local Registry') {
            steps {
                script {
                    docker.withRegistry("http://${DOCKER_REGISTRY}") {
                        docker.image("${DOCKER_REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}").push()
                        docker.image("${DOCKER_REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}").push('latest')
                    }
                }
            }
        }
        
        stage('Deploy with Docker Compose') {
            steps {
                sh '''
                    docker-compose down
                    docker-compose pull voting-app || true
                    docker-compose up -d --build
                '''
            }
        }
    }
    
    post {
        success {
            echo 'Pipeline completed successfully!'
            echo "Application deployed at: http://localhost:8080"
            echo "SonarQube dashboard: http://localhost:9000"
        }
        failure {
            echo 'Pipeline failed!'
        }
        always {
            cleanWs()
        }
    }
}
