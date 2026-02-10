pipeline {
    agent any
    
    tools {
        dotnetsdk 'dotnet-9.0'
    }
    
    environment {
        SONAR_HOST_URL = 'http://localhost:9000'
        DOCKER_REGISTRY = 'localhost:5000'
        IMAGE_NAME = 'voting-app'
        IMAGE_TAG = "${env.BUILD_ID}"
        DOTNET_CLI_HOME = "/tmp"  // Prevents permission issues
    }
    
    stages {
        stage('Checkout') {
            steps {
                git branch: 'main',
                    url: 'https://github.com/atechsphere/sample-voting-dotnet-app-v1.0.git'
            }
        }
        
        stage('Install Required Tools') {
            steps {
                script {
                    // Install SonarScanner for .NET
                    sh 'dotnet tool install --global dotnet-sonarscanner || true'
                    // Install ReportGenerator for coverage reports
                    sh 'dotnet tool install --global dotnet-reportgenerator-globaltool || true'
                }
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
                          --results-directory ./TestResults \
                          --settings coverlet.runsettings
                    '''
                    
                    // Process coverage reports (ensure correct path)
                    sh '''
                        reportgenerator \
                          -reports:./**/coverage.cobertura.xml \
                          -targetdir:./TestResults/CoverageReport \
                          -reporttypes:HtmlInline;Cobertura
                        
                        # Also create OpenCover format for SonarQube
                        reportgenerator \
                          -reports:./**/coverage.cobertura.xml \
                          -targetdir:./TestResults \
                          -reporttypes:OpenCover
                    '''
                }
            }
            
            post {
                always {
                    junit 'src/**/TestResults/*.trx'
                    publishHTML(target: [
                        reportDir: 'src/TestResults/CoverageReport',
                        reportFiles: 'index.html',
                        reportName: 'Code Coverage Report',
                        keepAll: true
                    ])
                }
            }
        }
        
        stage('SonarQube Analysis') {
            steps {
                dir('src') {
                    withSonarQubeEnv('SonarQube-Local') {
                        sh '''
                            # Start SonarQube analysis
                            dotnet sonarscanner begin \
                              /k:"sample-voting-dotnet-app-v1.0" \
                              /n:"Voting Application" \
                              /v:"${BUILD_ID}" \
                              /d:sonar.host.url="${SONAR_HOST_URL}" \
                              /d:sonar.cs.vstest.reportsPaths="**/TestResults/*.trx" \
                              /d:sonar.cs.opencover.reportsPaths="**/TestResults/*.opencover.xml" \
                              /d:sonar.coverage.exclusions="**Tests*.cs,**/Migrations/**" \
                              /d:sonar.exclusions="**/wwwroot/lib/**,**/node_modules/**" \
                              /d:sonar.sourceEncoding=UTF-8 \
                              /d:sonar.verbose=true
                            
                            # Build with SonarQube analysis
                            dotnet build VotingAppSolution.sln --configuration Release --no-restore
                            
                            # End SonarQube analysis
                            dotnet sonarscanner end
                        '''
                    }
                }
            }
        }
        
        stage('Build Docker Image') {
            steps {
                script {
                    // Build Docker image with runtime target
                    docker.build("${DOCKER_REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}", ".")
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
                    # Stop existing containers
                    docker-compose down || true
                    
                    # Pull latest image
                    docker-compose pull || true
                    
                    # Start new containers
                    docker-compose up -d --build
                    
                    # Wait for services to be ready
                    sleep 10
                    
                    # Show running containers
                    docker-compose ps
                '''
            }
        }
        
        stage('Health Check') {
            steps {
                script {
                    // Verify the application is running
                    sh '''
                        echo "Checking application health..."
                        curl --retry 5 --retry-delay 10 --retry-max-time 60 \
                             --max-time 30 \
                             -f http://localhost:8080/health || echo "Health check failed"
                    '''
                }
            }
        }
    }
    
    post {
        success {
            echo '🎉 Pipeline completed successfully!'
            echo "🌐 Application URL: http://localhost:8080"
            echo "📊 SonarQube Dashboard: ${SONAR_HOST_URL}/dashboard?id=sample-voting-dotnet-app-v1.0"
            echo "🐳 Docker Image: ${DOCKER_REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}"
            
            // Archive important artifacts
            archiveArtifacts artifacts: 'src/**/TestResults/*.trx', fingerprint: true
        }
        failure {
            echo '❌ Pipeline failed!'
            // Capture build logs for debugging
            sh '''
                echo "=== Docker Compose Logs ==="
                docker-compose logs --tail=50 || true
                echo "=== Docker Images ==="
                docker images | grep "${IMAGE_NAME}" || true
                echo "=== Running Containers ==="
                docker ps | grep "${IMAGE_NAME}" || true
            '''
        }
        unstable {
            echo '⚠️ Pipeline unstable - check test results!'
        }
        always {
            script {
                // Clean workspace safely within script block
                cleanWs(
                    cleanWhenAborted: true,
                    cleanWhenFailure: true, 
                    cleanWhenNotBuilt: true,
                    cleanWhenSuccess: true,
                    cleanWhenUnstable: true,
                    deleteDirs: true
                )
                
                // Optional: Remove dangling Docker images
                sh 'docker image prune -f || true'
            }
        }
    }
}
